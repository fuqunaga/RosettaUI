using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientKeysSwatch
    {
        public static readonly string CursorUSSClassName = $"{GradientEditor.USSClassName}__cursor";
     
        public static int TextDigit { get; set; } = 3;
        public static int MaxKeyNum { get; set; } = 8;
        
        private static float Round(float value, int digit)
        {
            var scale = Mathf.Pow(10f, digit);
            return Mathf.Round(value * scale) / scale;
        }
        
        public readonly bool isAlpha;
        public readonly VisualElement visualElement;

        public float Time
        {
            get => visualElement.style.left.value.value * 0.01f;
            set => visualElement.style.left = Length.Percent(Round(value * 100f, TextDigit));
        }

        public Color Color
        {
            get => visualElement.style.unityBackgroundImageTintColor.value;
            set => visualElement.style.unityBackgroundImageTintColor = value;
        }

        public float Alpha
        {
            get => visualElement.style.unityBackgroundImageTintColor.value.r;
            set
            {
                var v = Round(value, TextDigit);
                visualElement.style.unityBackgroundImageTintColor = new Color(v,v,v,1f);
            }
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