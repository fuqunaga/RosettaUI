using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Builder
{
    public static class ColorPickerHelper
    {
        public static Vector2Int defaultCheckerBoardSize = new(120, 25);
        public static int defaultCheckerBoardGridSize = 5;
        public static Vector2Int defaultSvTextureSize = new(140, 140);
        public static float defaultHueCircleThicknessRate = 0.1f;

        public static Texture2D CreateTexture(int width, int height, TextureFormat format = TextureFormat.RGB24) =>
            new(width, height, format, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
        
        
        #region CheckerBoard

        static Texture2D _checkerBoardTexture;

        public static Texture2D CheckerBoardTexture => _checkerBoardTexture ??=
            CreateCheckerBoardTexture(defaultCheckerBoardSize, defaultCheckerBoardGridSize, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));

        static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize, Color col0, Color col1)
        {
            var tex = CreateTexture(size.x, size.y);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            
            for (var y = 0; y < size.y; y++)
            {
                var flagY = ((y / gridSize) % 2 == 0);
                for (var x = 0; x < size.x; x++)
                {
                    var flagX = ((x / gridSize) % 2 == 0);
                    tex.SetPixel(x, y, (flagX ^ flagY) ? col0 : col1);
                }
            }

            tex.Apply();
            return tex;
        }

        #endregion

        #region Hue texture

        private static readonly Dictionary<float, Texture2D> HueCircleTextureDic = new();

        public static (Texture2D, int) GetHueCircleTextureAndThickness(float size)
        {
            var thickness = Mathf.RoundToInt(defaultHueCircleThicknessRate * size);
            
            if (!HueCircleTextureDic.TryGetValue(size, out var tex))
            {
                tex = HueCircleTextureDic[size] =  CreateHueTexture(Mathf.CeilToInt(size), thickness);
            }

            return (tex, thickness);
        }

        static Texture2D CreateHueTexture(int size, int circleThickness)
        {
            var thickness = circleThickness;
            
            var tex = CreateTexture(size, size, TextureFormat.RGBA32);

            var sizeHalf = size / 2;

            var minRadius = sizeHalf - thickness;
            var minRadiusSq = minRadius * minRadius;
            var maxRadius = sizeHalf;
            var maxRadiusSq = maxRadius * maxRadius;

            var transparent = Color.clear;
            var blendWidth = 2f;
            
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var pos = new Vector2(x - sizeHalf + 0.5f, y - sizeHalf + 0.5f);
                    
                    var color = transparent;

                    var radiusSq = pos.sqrMagnitude;
                    if  (minRadiusSq <= radiusSq && radiusSq <= maxRadiusSq)
                    {
                        var distance = pos.magnitude;
                        var alpha = Mathf.Min(
                            Mathf.InverseLerp(0f, blendWidth, distance - minRadius),
                            Mathf.InverseLerp(0f, blendWidth, maxRadius - distance)
                        );
                    
                    
                        var h = Mathf.Atan2(pos.y, pos.x) / (Mathf.PI * 2f);
                        if (h < 0f) h += 1f;
                        color = Color.HSVToRGB(h, 1f, 1f);
                        color.a = alpha;
                    }
                    
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        #endregion


        public static void UpdateSvTexture(Texture2D texture, float hue)
        {
            var width = texture.width;
            var height = texture.height;

            for (var y = 0; y < height; y++)
            {
                var v = (float)y / height;
                for (var x = 0; x < width; x++)
                {
                    var s = (float)x / width;
                    var c = Color.HSVToRGB(hue, s, v);
                    texture.SetPixel(x, y, c);
                }
            }

            texture.Apply();
        }
        
        static void FillArea(int xSize, int ySize, Color[] retval, Color topLeftColor, Color rightGradient, Color downGradient, bool convertToGamma)
        {
            // Calc the deltas for stepping.
            Color rightDelta = new Color(0, 0, 0, 0), downDelta  = new Color(0, 0, 0, 0);
            if (xSize > 1)
                rightDelta = rightGradient / (xSize - 1);
            if (ySize > 1)
                downDelta = downGradient / (ySize - 1);

            // Assign all colors into the array
            Color p = topLeftColor;
            int current = 0;
            for (int y = 0; y < ySize; y++)
            {
                Color p2 = p;
                for (int x = 0; x < xSize; x++)
                {
                    retval[current++] = convertToGamma ? p2.gamma : p2;
                    p2 += rightDelta;
                }
                p += downDelta;
            }
        }
    }
}