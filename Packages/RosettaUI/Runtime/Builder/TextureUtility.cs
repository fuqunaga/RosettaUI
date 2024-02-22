using System;
using System.Buffers;
using UnityEngine;

namespace RosettaUI.Builder
{
    public static class TextureUtility
    {
        public static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize)
            => CreateCheckerBoardTexture(size, gridSize, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));
        
        public static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize, Color col0, Color col1)
        {
            var tex = CreateTexture(size.x, size.y, TextureFormat.RGB24);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            var colorArray = ArrayPool<Color>.Shared.Rent(size.x * size.y);
            var colors = colorArray.AsSpan();

            for (var y = 0; y < size.y; y++)
            {
                var flagY = ((y / gridSize) % 2 == 0);
                for (var x = 0; x < size.x; x++)
                {
                    var flagX = ((x / gridSize) % 2 == 0);
                    colors[x + y * size.x] = (flagX ^ flagY) ? col0 : col1;
                }
            }

            tex.SetPixels(colorArray);
            ArrayPool<Color>.Shared.Return(colorArray);

            tex.Apply();
            return tex;
        }

        public static Texture2D CreateTexture(int width, int height, TextureFormat format = TextureFormat.RGBA32)
        {
            return new Texture2D(width, height, format, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
        }
    }
}