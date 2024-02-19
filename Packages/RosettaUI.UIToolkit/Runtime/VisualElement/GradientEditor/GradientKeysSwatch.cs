using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientKeysSwatch
    {
        public static readonly string CursorUSSClassName = $"{GradientEditor.USSClassName}__cursor";
        public static readonly string AlphaCursorUSSClassName = $"{GradientEditor.USSClassName}__cursor-alpha";
        public static readonly string ColorCursorUSSClassName = $"{GradientEditor.USSClassName}__cursor-color";
     
        public static int TextDigit { get; set; } = 3;
        
        private static float Round(float value, int digit)
        {
            var scale = Mathf.Pow(10f, digit);
            return Mathf.Round(value * scale) / scale;
        }
        
        public readonly VisualElement visualElement;
        
        private bool _isAlpha;

        public bool IsAlpha
        {
            get => _isAlpha; 
            private set
            {
                _isAlpha = value;
                if (_isAlpha)
                {
                    visualElement.RemoveFromClassList(ColorCursorUSSClassName);
                    visualElement.AddToClassList(AlphaCursorUSSClassName);
                }
                else
                {
                    visualElement.RemoveFromClassList(AlphaCursorUSSClassName);
                    visualElement.AddToClassList(ColorCursorUSSClassName);
                }
            }
        }
        

        public float Time
        {
            get => visualElement.style.left.value.value * 0.01f;
            set => visualElement.style.left = Length.Percent(Round(value * 100f, TextDigit));
        }

        public Color Color
        {
            get => visualElement.style.unityBackgroundImageTintColor.value;
            set
            {
                visualElement.style.unityBackgroundImageTintColor = value;
                IsAlpha = false;
            }
        }

        public float Alpha
        {
            get => visualElement.style.unityBackgroundImageTintColor.value.r;
            set
            {
                var v = Round(value, TextDigit);
                visualElement.style.unityBackgroundImageTintColor = new Color(v,v,v,1f);
                IsAlpha = true;
            }
        }

        public GradientKeysSwatch()
        { 
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