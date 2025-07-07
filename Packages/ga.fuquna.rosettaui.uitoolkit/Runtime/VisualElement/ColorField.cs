using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorField : ColorFieldBase
    {
        public static readonly string OverlapTextUssClassName = ussClassName + "__overlap-text";
        public static readonly string OverlapTextLightUssClassName = OverlapTextUssClassName + "--light";
        public static readonly string OverlapTextDarkUssClassName = OverlapTextUssClassName + "--dark";

        public TextElement OverlapTextElement { get; protected set; }
        
        public ColorField() : this(null) { }
        
        public ColorField(string label) : base(label)
        {
            OverlapTextElement = new TextElement();
            OverlapTextElement.AddToClassList(OverlapTextUssClassName);
            
            colorInput.Add(OverlapTextElement);

            UpdateOverlapText();
        }
        
        public override void SetValueWithoutNotify(Color color)
        {
            base.SetValueWithoutNotify(color);
            UpdateOverlapText();
        }


        void UpdateOverlapText()
        {
            var color = value;
            
            Color.RGBToHSV(color, out var h, out var s, out var v);
            OverlapTextElement.text = $" HSV {h:0.00} {s:0.00} {v:0.00}";

            var yuvY = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
            var darkText = yuvY >= 0.4f;

            if (darkText)
            {
                OverlapTextElement.RemoveFromClassList(OverlapTextLightUssClassName);
                OverlapTextElement.AddToClassList(OverlapTextDarkUssClassName);
            }
            else
            {
                OverlapTextElement.RemoveFromClassList(OverlapTextDarkUssClassName);
                OverlapTextElement.AddToClassList(OverlapTextLightUssClassName);
            }
        }
    }
}