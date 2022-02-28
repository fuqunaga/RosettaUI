using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace RosettaUI.Builder
{
    public static class ColorPickerHelper
    {
        public static Vector2Int defaultCheckerBoardSize = new(100, 25);
        public static int defaultCheckerBoardGridSize = 5;
        public static float defaultHueCircleThicknessRate = 0.1f;

        public static string svDiskMaterialPath = "SvDisk";

        public static Texture2D CreateTexture(int width, int height, TextureFormat format = TextureFormat.RGBA32) =>
            new(width, height, format, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

        public static RenderTexture CreateRenderTexture(int width, int height) => 
            new(width, height, 0)
            {
                wrapMode = TextureWrapMode.Clamp
            };
        
        #region CheckerBoard

        static Texture2D _checkerBoardTexture;

        public static Texture2D CheckerBoardTexture => _checkerBoardTexture ??= CreateCheckerBoardTexture(defaultCheckerBoardSize, defaultCheckerBoardGridSize);

        public static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize)
            => CreateCheckerBoardTexture(size, gridSize, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));
        
        static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize, Color col0, Color col1)
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

            var colorArray = ArrayPool<Color>.Shared.Rent(size * size);
            var colors = colorArray.AsSpan();

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

                    colors[x + y * size] = color;
                }
            }

            tex.SetPixels(colorArray);

            ArrayPool<Color>.Shared.Return(colorArray);

            tex.Apply();
            return tex;
        }

        #endregion

        #region SV texture

        private static class SvCircleShaderParam
        {
            public static readonly int TargetSize = Shader.PropertyToID("_TargetSize");
            public static readonly int BlendWidthNormalized = Shader.PropertyToID("_BlendWidthNormalized");
            public static readonly int Hue = Shader.PropertyToID("_Hue");
        }
        
        private static Material _svDiskMaterial;

        /// <summary>
        /// 横軸S縦軸Vの正方形でのSVを計算し、それを円形にマップする
        /// </summary>
        public static void UpdateSvDiskTexture(RenderTexture rt, float hue)
        {
#if true
            _svDiskMaterial ??= Resources.Load<Material>(svDiskMaterialPath);

            _svDiskMaterial.SetVector(SvCircleShaderParam.TargetSize, new Vector2(rt.width, rt.height));
            _svDiskMaterial.SetFloat(SvCircleShaderParam.BlendWidthNormalized, 2f / rt.width);
            _svDiskMaterial.SetFloat(SvCircleShaderParam.Hue, hue);

            var tmp = RenderTexture.active;
            RenderTexture.active = rt;
            
            GL.PushMatrix();
            GL.LoadOrtho();
            
            _svDiskMaterial.SetPass(0);
            
            GL.Begin(GL.QUADS);
            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f);
            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 0.0f);
            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);
            GL.End();
            GL.PopMatrix();

            RenderTexture.active = tmp;

#else
            Assert.AreEqual(texture.width, texture.height);
            
            var size = texture.width;
            var radius = size * 0.5f;
            var center = Vector2.one * radius;
            
            var blendWidth = 1f;
            var blendWidthNormalized = blendWidth / radius;

            var colorArray = ArrayPool<Color>.Shared.Rent(size * size);
            var colors = colorArray.AsSpan();

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

                    colors[x + y * size] = color;
                }
            }
            
            texture.SetPixels(colorArray);
            
            ArrayPool<Color>.Shared.Return(colorArray);

            texture.Apply();
#endif
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
        
        #endregion


        #region Slider

        public static void WriteHueSliderTexture(Texture2D tex)
        {
            WriteSliderTexture(tex, rate => Color.HSVToRGB(rate, 1f, 1f));
        }

        public static void UpdateSSliderTexture(Texture2D tex, float hue, float v)
        {
            var width = tex.width;

            // v==0でも真っ黒にしない
            // Unity のカラーピッカーの挙動参考
            var lerpV = Mathf.Lerp(0.2f, 1f, v);

            WriteSliderTexture(tex, rate => Color.HSVToRGB(hue, rate, lerpV));
        }
        
        public static void UpdateVSliderTexture(Texture2D tex, float hue, float s)
        {
            WriteSliderTexture(tex, rate => Color.HSVToRGB(hue, s, rate));
        }

        public static void UpdateRSliderTexture(Texture2D tex, Color color) => UpdateRgbSliderTexture(tex, color, 0);
        public static void UpdateGSliderTexture(Texture2D tex, Color color) => UpdateRgbSliderTexture(tex, color, 1);
        public static void UpdateBSliderTexture(Texture2D tex, Color color) => UpdateRgbSliderTexture(tex, color, 2);
        
        static void UpdateRgbSliderTexture(Texture2D tex, Color color, int index)
        {
            WriteSliderTexture(tex, rate =>
            {
                color[index] = rate;
                return color;
            });
        }
        

        static void WriteSliderTexture(Texture2D tex, Func<float, Color> rateToColor)
        {
            var width = tex.width;
            var invWidth = 1f / width;
            
            var colorArray = ArrayPool<Color>.Shared.Rent(width);
            var colors = colorArray.AsSpan();

            for (var x = 0; x < width; x++)
            {
                var rate = (x + 0.5f) * invWidth;
                colors[x] = rateToColor(rate);
            }

            tex.SetPixels(colorArray);
            ArrayPool<Color>.Shared.Return(colorArray);
            tex.Apply();
        }

        #endregion

        #region Hex

        public static Color32? HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return null;
        
            hex = hex.Replace ("0x", ""); //in case the string is formatted 0xFFFFFF
            hex = hex.Replace ("#", "");  //in case the string is formatted #FFFFFF

            if (hex.Length != 6) return null;
            
            var provider = CultureInfo.InvariantCulture;
            
           if ( !byte.TryParse(hex.AsSpan(0,2), NumberStyles.HexNumber, provider, out var r) ) return null;
           if ( !byte.TryParse(hex.AsSpan(2,2), NumberStyles.HexNumber, provider, out var g) ) return null;
           if ( !byte.TryParse(hex.AsSpan(4,2), NumberStyles.HexNumber, provider, out var b) ) return null;

            return new Color32(r,g,b,0);
        }

        public static string ColorToHex(Color32 color)
            => color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        
        #endregion
        
#if false

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
        
#endif
    }
}