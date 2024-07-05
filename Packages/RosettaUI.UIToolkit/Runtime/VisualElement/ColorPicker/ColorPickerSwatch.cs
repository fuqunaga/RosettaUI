using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatch : VisualElement
    {
        public const string UssClassName = "rosettaui-colorpicker-swatch";
        public const string CurrentClassName = UssClassName + "-current";
        public const string CoreClassName = UssClassName + "-core";
        public const string ColorUssClassName = UssClassName + "-color";
        public const string ColorOpaqueUssClassName = UssClassName + "-color-opaque";

        private readonly VisualElement _coreElement;
        private readonly VisualElement _colorElement;
        private readonly VisualElement _colorOpaque;

        public bool IsCurrent
        {
            set
            {
                if (value)
                {
                    AddToClassList(CurrentClassName);
                }
                else
                {
                    RemoveFromClassList(CurrentClassName);
                }
                
            }
        }
        
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
            
            _coreElement = new VisualElement();
            _coreElement.AddToClassList(CoreClassName);
            
            var checkerboardElement = new Checkerboard(CheckerboardTheme.Light);

            _colorElement = new VisualElement();
            _colorElement.AddToClassList(ColorUssClassName);

            _colorOpaque = new VisualElement();
            _colorOpaque.AddToClassList(ColorOpaqueUssClassName);

            _colorElement.Add(_colorOpaque);
            checkerboardElement.Add(_colorElement);
            _coreElement.Add(checkerboardElement);
            
            Add(_coreElement);

            IsCurrent = true;
        }
    }
}