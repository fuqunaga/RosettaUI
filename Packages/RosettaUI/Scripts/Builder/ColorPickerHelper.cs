using UnityEngine;

namespace RosettaUI.Builder
{
    public static class ColorPickerHelper
    {
        public static Vector2Int defaultCheckerBoardSize = new Vector2Int(312, 24);
        public static Vector2Int defaultSvTextureSize = new Vector2Int(140, 140);
        public static Vector2Int defaultHueTextureSize = new Vector2Int(280, 24);

        #region CheckerBoard

        static Texture2D _checkerBoardTexture;

        public static Texture2D CheckerBoardTexture => _checkerBoardTexture ??=
            CreateCheckerBoardTexture(defaultCheckerBoardSize, 4, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));

        static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize, Color col0, Color col1)
        {
            var tex = new Texture2D(size.x, size.y);
            for (var y = 0; y < size.y; y++)
            {
                var flagY = ((y / gridSize) % 2 == 0);
                for (var x = 0; x < size.x; x++)
                {
                    var flagX = ((x / gridSize) % 2 == 0);
                    tex.SetPixel(x, y, (flagX ^ flagY) ? col0 : col1);
                }
            }

            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();
            return tex;
        }

        #endregion

        #region Hue texture

        private static Texture2D _hueTexture;
        public static Texture2D HueTexture => _hueTexture ??= CreateHueTexture(defaultHueTextureSize);

        static Texture2D CreateHueTexture(Vector2Int size)
        {
            var width = size.x;
            var height = size.y;

            var tex = new Texture2D(width, height);
            for (var y = 0; y < height; y++)
            {
                var h = 1f * y / height;
                var color = Color.HSVToRGB(h, 1f, 1f);
                for (var x = 0; x < width; x++)
                {
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

#if true
            // optimize ref: Color.HSVToRGB()
            float f = hue * 6f;
            int num3 = (int) Mathf.Floor(f);
            float num4 = f - (float) num3;

            for (var x = 0; x < width; x++)
            {
                var s = (float) x / width;
                float num1 = s;
                float num5 = /* num2 * */(1f - num1);
                float num6 = /* num2 * */(float) (1.0 - (double) num1 * (double) num4);
                float num7 = /* num2 * */(float) (1.0 - (double) num1 * (1.0 - (double) num4));

                var rgbBase = num3 switch
                {
                    -1 => new Vector3(1f, num5, num6),
                    0 => new Vector3(1f, num7, num5),
                    1 => new Vector3(num6, 1f, num5),
                    2 => new Vector3(num5, 1f, num7),
                    3 => new Vector3(num5, num6, 1f),
                    4 => new Vector3(num7, num5, 1f),
                    5 => new Vector3(1f, num5, num6),
                    6 => new Vector3(1f, num7, num6),
                    _ => default
                };

                for (var y = 0; y < height; y++)
                {
                    var v = (float) y / height;
                    float num2 = v;
                    var rgb = rgbBase * num2;
                    texture.SetPixel(x, y, new Color(rgb.x, rgb.y, rgb.z));
                }
            }

#else
            // base code
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
#endif

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