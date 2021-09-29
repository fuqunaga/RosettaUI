using UnityEngine;

namespace RosettaUI.Builder
{
    public static class ColorPickerHelper
    {
        public static Vector2Int defaultCheckerBoardSize = new Vector2Int(312, 24);
        public static Vector2Int defaultSvTextureSize = new Vector2Int(280, 280);
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
                for (int x = 0; x < width; x++)
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
            var size = texture.width;
            for (var y = 0; y < size; y++)
            {
                var v = 1f * y / size;
                for (var x = 0; x < size; x++)
                {
                    var s = 1f * x / size;
                    var c = Color.HSVToRGB(hue, s, v);
                    texture.SetPixel(x, y, c);
                }
            }

            texture.Apply();
        }
    }
}