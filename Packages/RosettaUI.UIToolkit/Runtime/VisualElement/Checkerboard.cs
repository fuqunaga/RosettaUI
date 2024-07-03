using RosettaUI.Builder;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Checkerboard : VisualElement
    {
        public const int DefaultSize = 8;
        public const string UssClassName = "rosettaui-checkerboard";

        public static void SetupAsCheckerboard(VisualElement visualElement, CheckerboardTheme theme, int size = DefaultSize)
        {
            visualElement.AddToClassList(UssClassName);
            visualElement.style.backgroundImage = TextureUtility.GetOrCreateCheckerboardTexture2x2(theme);
            visualElement.style.backgroundSize = new BackgroundSize(DefaultSize, DefaultSize);
        }

        public Checkerboard(CheckerboardTheme theme, int size = DefaultSize)
        {
            SetupAsCheckerboard(this, theme, size);
        }
    }
}