using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatch : VisualElement
    {
        public const string UssClassName = "rosettaui-colorpicker-swatch";
        public const string ColorUssClassName = UssClassName + "__color";
        public const string ColorOpaqueUssClassName = UssClassName + "__color-opaque";

        private readonly VisualElement _colorElement;
        private readonly VisualElement _colorOpaque;
        
        

        public Color Color
        {
            get => _colorElement.style.backgroundColor.value;
            set
            {
                _colorElement.style.backgroundColor = value;
                
                var colorWithoutAlpha = value;
                colorWithoutAlpha.a = 1;
                _colorOpaque.style.unityBackgroundImageTintColor = colorWithoutAlpha;
            }
    }
        
        public ColorPickerSwatch()
        {
            AddToClassList(UssClassName);
            
            var checkerboardElement = new Checkerboard(CheckerboardTheme.Light);

            _colorElement = new VisualElement();
            _colorElement.AddToClassList(ColorUssClassName);

            _colorOpaque = new VisualElement();
            _colorOpaque.AddToClassList(ColorOpaqueUssClassName);

            _colorElement.Add(_colorOpaque);
            checkerboardElement.Add(_colorElement);
            Add(checkerboardElement);
        }
    }
}