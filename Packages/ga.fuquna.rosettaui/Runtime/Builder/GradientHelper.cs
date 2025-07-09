using System;
using System.Buffers;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace RosettaUI.Builder
{
    public static class GradientHelper
    {
        private delegate IntPtr GradientBindingsMarshallerConvertToNative(Gradient gradient);
        private delegate bool GradientInternalEquals(Gradient gradient, IntPtr other);

        private static GradientBindingsMarshallerConvertToNative _convertToNativeFunc;
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
        /// アロケーション無しで値での比較
        /// 以下の操作はアロケーションが発生するので、Gradient内部の比較メソッドを利用する
        /// - Gradient.colorKeys
        /// - Gradient.Equals()
        /// </summary>
        public static bool EqualsByValue(Gradient gradient, Gradient other)
        {
            if (ReferenceEquals(gradient, other)) return true;
            if (gradient is null || other is null) return false;

            if (_convertToNativeFunc == null)
            {
                var marshallerType = typeof(Gradient).GetNestedType("BindingsMarshaller", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var method = marshallerType.GetMethod("ConvertToNative", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                Assert.IsNotNull(method, "Gradient.BindingsMarshaller.ConvertToNative method not found.");
                _convertToNativeFunc = (GradientBindingsMarshallerConvertToNative)Delegate.CreateDelegate(typeof(GradientBindingsMarshallerConvertToNative), method);
            }
            
            if (_internalEqualsFunc == null)
            {
                var method = typeof(Gradient).GetMethod("Internal_Equals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(method, "Gradient.Internal_Equals method not found.");
                _internalEqualsFunc = (GradientInternalEquals)Delegate.CreateDelegate(typeof(GradientInternalEquals), method);
            }
            
            return _internalEqualsFunc(gradient, _convertToNativeFunc(other));
        }
        
        public static Texture2D GenerateOrUpdatePreviewTexture(Gradient gradient, Texture2D texture)
        {
            const int width = 256;
            const int height = 1;
            if (texture != null && (texture.width != width || texture.height != height))
            {
                Object.Destroy(texture);
            }
            
            var  colorArray = ArrayPool<Color>.Shared.Rent(width);
            for (var i = 0; i < width; i++)
            {
                colorArray[i] = gradient.Evaluate(i / (float)width);
            }
            
            texture ??= new Texture2D(width, height)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels(0, 0, width, height, colorArray);
            texture.Apply();
            
            ArrayPool<Color>.Shared.Return(colorArray);
            
            return texture;
        }
    }
}
