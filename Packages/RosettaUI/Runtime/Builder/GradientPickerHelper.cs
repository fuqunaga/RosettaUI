using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace RosettaUI.Builder
{
    public static class GradientPickerHelper 
    {

        public static Texture2D GenerateGradientPreview(Gradient gradient, Texture2D texture)
        {
            int width = 256;
            if (texture != null)
            {
                width = texture.width;
                Object.Destroy(texture);
            }
            Color[] g = new Color[width];
            for (int i = 0; i < g.Length; i++)
            {
                g[i] = gradient.Evaluate(i / (float)g.Length);
            }
            
            Texture2D tex = new Texture2D(g.Length, 1)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            tex.SetPixels(g);
            tex.Apply();
            return tex;
        }

        [System.Serializable]
        public class SerializedAlphaKey
        {
            public float t;
            public float a;
        }
        
        [System.Serializable]
        public class SerializedColorKey
        {
            public float t;
            public Color c;
        }
            
        [System.Serializable]
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
            
            var code = new GradientCode();
            code.alphaKeys = new SerializedAlphaKey[gradient.alphaKeys.Length];
            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                code.alphaKeys[i] = new SerializedAlphaKey()
                {
                    t = gradient.alphaKeys[i].time,
                    a = gradient.alphaKeys[i].alpha
                };
            }
            code.colorKeys = new SerializedColorKey[gradient.colorKeys.Length];
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                code.colorKeys[i] = new SerializedColorKey()
                {
                    t = gradient.colorKeys[i].time,
                    c = gradient.colorKeys[i].color
                };
            }
            code.mode = gradient.mode;

            try
            {
                codeString = JsonUtility.ToJson(code);
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
            }

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
            catch(System.Exception e)
            {
                Debug.LogError(e);
            }

            return gradient;
        }
    }
}
