using System;
using System.Buffers;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace RosettaUI.Builder
{
    public static class GradientPickerHelper 
    {

        public static Texture2D GenerateGradientPreview(Gradient gradient, Texture2D texture)
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
        

        [Serializable]
        public class SerializedAlphaKey
        {
            public float t;
            public float a;
        }
        
        [Serializable]
        public class SerializedColorKey
        {
            public float t;
            public Color c;
        }
            
        [Serializable]
        public class GradientCode
        {
            public SerializedAlphaKey[] alphaKeys;
            public SerializedColorKey[] colorKeys;
            public GradientMode mode;
        }
        
        [CanBeNull]
        public static string GradientToJson(Gradient gradient)
        {
            string codeString = null;

            var code = GenericPool<GradientCode>.Get();
            
            // ArrayPoolの最小値が16なのでJson化した時に無駄なデータが出て、復元時に要素数オーバーでエラーになるため、仕方なくnewする
            code.alphaKeys = new SerializedAlphaKey[gradient.alphaKeys.Length]; 
            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                code.alphaKeys[i] = GenericPool<SerializedAlphaKey>.Get();
                code.alphaKeys[i].t = gradient.alphaKeys[i].time;
                code.alphaKeys[i].a = gradient.alphaKeys[i].alpha;
            }

            // ArrayPoolの最小値が16なのでJson化した時に無駄なデータが出て、復元時に要素数オーバーでエラーになるため、仕方なくnewする
            code.colorKeys = new SerializedColorKey[gradient.colorKeys.Length];
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                code.colorKeys[i] = GenericPool<SerializedColorKey>.Get();
                code.colorKeys[i].t = gradient.colorKeys[i].time;
                code.colorKeys[i].c = gradient.colorKeys[i].color;
            }
            code.mode = gradient.mode;

            try
            {
                codeString = JsonUtility.ToJson(code);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            // release
            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                GenericPool<SerializedAlphaKey>.Release( code.alphaKeys[i]);
            }
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                GenericPool<SerializedColorKey>.Release(code.colorKeys[i]);
            }
            GenericPool<GradientCode>.Release(code);
            return codeString;
        }

        [CanBeNull]
        public static Gradient JsonToGradient(string json)
        {
            Gradient gradient = null;
            
            try
            {
                var code = JsonUtility.FromJson<GradientCode>(json);
                if (code != null)
                {
                    gradient = new Gradient();
                    var alphaKeys = new GradientAlphaKey[code.alphaKeys.Length];
                    for (int i = 0; i < code.alphaKeys.Length; i++)
                    {
                        alphaKeys[i] = new GradientAlphaKey()
                        {
                            time = code.alphaKeys[i].t,
                            alpha = code.alphaKeys[i].a
                        };
                    }

                    var colorKeys = new GradientColorKey[code.colorKeys.Length];
                    for (int i = 0; i < code.colorKeys.Length; i++)
                    {
                        colorKeys[i] = new GradientColorKey()
                        {
                            time = code.colorKeys[i].t,
                            color = code.colorKeys[i].c
                        };
                    }

                    gradient.mode = code.mode;
                    gradient.SetKeys(colorKeys, alphaKeys);
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            return gradient;
        }
    }
}
