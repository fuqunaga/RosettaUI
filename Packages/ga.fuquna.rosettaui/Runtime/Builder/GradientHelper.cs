using System;
using System.Buffers;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

#if !UNITY_6000_0_OR_NEWER
using System.Linq.Expressions;
#endif


namespace RosettaUI.Builder
{
    public static class GradientHelper
    {
        private delegate IntPtr GradientBindingsMarshallerConvertToNative(Gradient gradient);
        private static GradientBindingsMarshallerConvertToNative _convertToNativeFunc;
        
        private delegate bool GradientInternalEquals(Gradient gradient, IntPtr other);
        private static GradientInternalEquals _internalEqualsFunc;
        
        
        public static Gradient Clone(Gradient src)
        {
            var dst = new Gradient();
            Copy(src, dst);
            return dst;
        }
        
        public static void Copy(Gradient src, Gradient dst)
        {
            dst.SetKeys(src.colorKeys, src.alphaKeys);
            dst.mode = src.mode;
            dst.colorSpace = src.colorSpace;
        }
        
        /// <summary>
        /// Unity6以降ではアロケーション無しで値での比較
        /// ＊たまにInternal_Equals 内でアロケーションするっぽい
        /// 以下の操作はアロケーションが発生するので、Gradient内部の比較メソッドを利用する
        /// - Gradient.colorKeys
        /// - Gradient.Equals() : boxingが発生するため
        /// </summary>
        public static bool EqualsWithoutAlloc(Gradient gradient, Gradient other)
        {
            if (ReferenceEquals(gradient, other)) return true;
            if (gradient is null || other is null) return false;

            if (_convertToNativeFunc == null)
            {
#if UNITY_6000_0_OR_NEWER
                var marshallerType = typeof(Gradient).GetNestedType("BindingsMarshaller", BindingFlags.NonPublic | BindingFlags.Static);
                var method = marshallerType.GetMethod("ConvertToNative", BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(method, "Gradient.BindingsMarshaller.ConvertToNative method not found.");
                
                _convertToNativeFunc = (GradientBindingsMarshallerConvertToNative)Delegate.CreateDelegate(typeof(GradientBindingsMarshallerConvertToNative), method);
#else
                // Unity6以前のバージョンではConvertToNativeメソッドがない
                // FieldInfo.GetValue()はアロケーション(boxing)が発生するため、Expressionを使う
                var targetParam = Expression.Parameter(typeof(Gradient), "target");
                var field = typeof(Gradient).GetField("m_Ptr", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(field, "Gradient.m_Ptr field not found.");
                var fieldAccess = Expression.Field(targetParam, field);
                
                var lambda = Expression.Lambda<GradientBindingsMarshallerConvertToNative>(
                    fieldAccess,
                    targetParam
                );
                
                _convertToNativeFunc = lambda.Compile();
#endif
            }

            if (_internalEqualsFunc == null)
            {
                var method = typeof(Gradient).GetMethod("Internal_Equals", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method, "Gradient.Internal_Equals method not found.");
                _internalEqualsFunc = (GradientInternalEquals)Delegate.CreateDelegate(typeof(GradientInternalEquals), method);
            }
            
            return _internalEqualsFunc(gradient, _convertToNativeFunc(other));
        }
        
        public static Texture2D GenerateOrUpdatePreviewTexture(Gradient gradient, Texture2D texture)
        {
            const int width = 256;
            const int height = 1;
            
            if (TextureUtility.EnsureTextureSize(ref texture, width, height))
            {
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
            }
            
            var  colorArray = ArrayPool<Color>.Shared.Rent(width);
            for (var i = 0; i < width; i++)
            {
                colorArray[i] = gradient.Evaluate(i / (float)width);
            }
      
            texture.SetPixels(0, 0, width, height, colorArray);
            texture.Apply();
            
            ArrayPool<Color>.Shared.Return(colorArray);
            
            return texture;
        }
    }
}
