using System.Buffers;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RosettaUI.Builder
{
    public static class GradientHelper 
    {
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
    }
}
