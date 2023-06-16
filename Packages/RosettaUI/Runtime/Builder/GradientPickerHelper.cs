using System.Collections;
using System.Collections.Generic;
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
    }
}
