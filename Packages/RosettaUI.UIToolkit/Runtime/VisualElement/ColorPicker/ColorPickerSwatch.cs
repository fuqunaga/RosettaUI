using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatch : VisualElement
    {
        public const string UssClassName = "rosettaui-colorpicker-swatch";
        public const string TileClassName = UssClassName + "__tile";
        public const string TileCurrentClassName = TileClassName + "-current";
        public const string TileCoreClassName = TileClassName + "-core";
        public const string TileColorUssClassName = TileClassName + "-color";
        public const string TileColorOpaqueUssClassName = TileClassName + "-color-opaque";
        
        private readonly VisualElement _tileElement;
        private readonly VisualElement _colorElement;
        private readonly VisualElement _colorOpaque;

        private Label _label;

        public bool IsCurrent
        {
            set
            {
                if (value)
                {
                    _tileElement.AddToClassList(TileCurrentClassName);
                }
                else
                {
                    _tileElement.RemoveFromClassList(TileCurrentClassName);
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

        public string Label
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if ( _label != null) Remove(_label);
                }
                else
                {
                    _label ??= new Label();

                    _label.text = value;
                    Add(_label);
                }
            }
        }
        
        public ColorPickerSwatch()
        {
            AddToClassList(UssClassName);
            
            _tileElement = new VisualElement();
            _tileElement.AddToClassList(TileClassName);
            
            var coreElement = new VisualElement();
            coreElement.AddToClassList(TileCoreClassName);
            
            var checkerboardElement = new Checkerboard(CheckerboardTheme.Light);

            _colorElement = new VisualElement();
            _colorElement.AddToClassList(TileColorUssClassName);

            _colorOpaque = new VisualElement();
            _colorOpaque.AddToClassList(TileColorOpaqueUssClassName);

            _colorElement.Add(_colorOpaque);
            checkerboardElement.Add(_colorElement);
            coreElement.Add(checkerboardElement);
            _tileElement.Add(coreElement);
            
            Add(_tileElement);
        }
    }
}