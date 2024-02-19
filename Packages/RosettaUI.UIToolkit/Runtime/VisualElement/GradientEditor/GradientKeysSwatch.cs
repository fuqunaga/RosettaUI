using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientKeysSwatch
    {
        public static readonly string CursorUSSClassName = $"{GradientEditor.USSClassName}__cursor";
        
        public readonly bool isAlpha;
        public readonly VisualElement visualElement;

        public float Time
        {
            get => visualElement.style.left.value.value * 0.01f;
            set => visualElement.style.left = Length.Percent(value * 100f);
        }

        public Color Color
        {
            get => visualElement.style.unityBackgroundImageTintColor.value;
            set => visualElement.style.unityBackgroundImageTintColor = value;
        }

        public GradientKeysSwatch(bool isAlpha)
        {
            this.isAlpha = isAlpha;
            visualElement = CreateVisualElement();
        }

        private static VisualElement CreateVisualElement()
        {
            var cursor = new VisualElement();
            cursor.AddToClassList(CursorUSSClassName);
            cursor.focusable = true;
            return cursor;
        }
    }
}