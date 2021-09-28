using UnityEngine;

namespace RosettaUI
{
    public static class ColorPickerHelper
    {
        #region CheckerBoard

        static Texture2D _checkerBoardTexture;
        public static Texture2D CheckerBoardTexture => _checkerBoardTexture ??= CreateCheckerBoardTexture(new Vector2Int(200, 18), 4, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));
        
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
        
        
        public static void UpdateSVTexture(Texture2D texture, float hue)
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
