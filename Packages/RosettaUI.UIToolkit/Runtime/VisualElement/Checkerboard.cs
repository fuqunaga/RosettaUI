using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Checkerboard : VisualElement
    {
        public const string UssClassName = "rosettaui-checkerboard";

        public static Texture2D CheckerBoardTexture2X2 { get; private set; }
        

        public Checkerboard(int size = 12, Texture2D texture = null)
        {
            AddToClassList(UssClassName);

            if (texture == null)
            {
                CheckerBoardTexture2X2 ??= TextureUtility.CreateCheckerBoardTexture(new Vector2Int(2, 2), 1);
                texture = CheckerBoardTexture2X2;
            }

            style.backgroundImage = texture;
            style.backgroundSize = new BackgroundSize(size, size);
        }
    }
}