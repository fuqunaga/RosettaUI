using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace RosettaUI.Builder
{
    public static class ColorPickerHelper
    {
        public static Vector2Int defaultCheckerBoardSize = new(120, 25);
        public static int defaultCheckerBoardGridSize = 5;
        public static Vector2Int defaultSvTextureSize = new(140, 140);
        public static float defaultHueCircleThicknessRate = 0.1f;

        public static Texture2D CreateTexture(int width, int height, TextureFormat format = TextureFormat.RGBA32) =>
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
            var tex = CreateTexture(size.x, size.y, TextureFormat.RGB24);
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
            
            var tex = CreateTexture(size, size);

            var sizeHalf = size / 2;

            var minRadius = sizeHalf - thickness;
            var minRadiusSq = minRadius * minRadius;
            var maxRadius = sizeHalf;
            var maxRadiusSq = maxRadius * maxRadius;

            var blendWidth = 2f;
            
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var pos = new Vector2(x - sizeHalf + 0.5f, y - sizeHalf + 0.5f);
                    
                    var color = Color.clear;;

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


        /// <summary>
        /// 横軸S縦軸Vの正方形でのSVを計算し、それを円形にマップする
        /// </summary>
        public static void UpdateSvCircleTexture(Texture2D texture, float hue)
        {
            Assert.AreEqual(texture.width, texture.height);
            
            var size = texture.width;
            var radius = size * 0.5f;
            var center = Vector2.one * radius;
            
            var blendWidth = 1f;
            var blendWidthNormalized = blendWidth / radius;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var color = Color.clear;
                    
                    var pos = (new Vector2(x + 0.5f, y + 0.5f) - center) / radius;
                    if (pos.sqrMagnitude <= 1f)
                    {
                        var posOnSquare = CircleToSquare(pos);
                        var sv = (posOnSquare + Vector2.one) * 0.5f; // map -1~1 > 0~1

                        color = Color.HSVToRGB(hue, sv.x, sv.y);
                        color.a = Mathf.InverseLerp(1f, 1f - blendWidthNormalized, pos.magnitude);
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
        }


        
        static readonly float TwoSqrt2 = 2f * Mathf.Sqrt(2f);
        
        /// <summary>
        /// 半径１の円内の座標を１辺２(-1~1)の正方形に射影する
        /// http://squircular.blogspot.com/2015/09/mapping-circle-to-square.html
        /// </summary>
        public static Vector2 CircleToSquare(Vector2 uv)
        {
            var u = uv.x;
            var v = uv.y;

            var u2 = u * u;
            var v2 = v * v;

            var subTermX = 2f + u2 - v2;
            var subTermY = 2f - u2 + v2;
            var termX1 = subTermX + u * TwoSqrt2;
            var termX2 = subTermX - u * TwoSqrt2;
            var termY1 = subTermY + v * TwoSqrt2;
            var termY2 = subTermY - v * TwoSqrt2;

            return new Vector2(
                0.5f * Mathf.Sqrt(termX1) - 0.5f * Mathf.Sqrt(termX2),
                0.5f * Mathf.Sqrt(termY1) - 0.5f * Mathf.Sqrt(termY2)
            );
        }

        /// <summary>
        /// １辺２(-1~1)の正方形内の座標を半径１の円に射影する
        /// http://squircular.blogspot.com/2015/09/mapping-circle-to-square.html
        /// </summary>
        public static Vector2 SquareToCircle(Vector2 xy)
        {
            var x = xy.x;
            var y = xy.y;
            return new Vector2(
                x * Mathf.Sqrt(1f - y * y * 0.5f),
                y * Mathf.Sqrt(1f - x * x * 0.5f)
            );
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