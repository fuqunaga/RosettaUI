using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatch : SwatchBase<Color>
    {
        public new const string UssClassName = "rosettaui-colorpicker-swatch";
        public new const string TileClassName = UssClassName + "__tile";
        public const string TileCoreClassName = TileClassName + "-core";
        public const string TileColorUssClassName = TileClassName + "-color";
        public const string TileColorOpaqueUssClassName = TileClassName + "-color-opaque";

        private readonly VisualElement _colorElement;
        private readonly VisualElement _opaqueElement;
   
        public ColorPickerSwatch()
        {
            var tileElement = new VisualElement();
            tileElement.AddToClassList(TileClassName);
            
            var coreElement = new VisualElement();
            coreElement.AddToClassList(TileCoreClassName);
            
            var checkerboardElement = new Checkerboard(CheckerboardTheme.Light);

            _colorElement = new VisualElement();
            _colorElement.AddToClassList(TileColorUssClassName);

            _opaqueElement = new VisualElement();
            _opaqueElement.AddToClassList(TileColorOpaqueUssClassName);

            _colorElement.Add(_opaqueElement);
            checkerboardElement.Add(_colorElement);
            coreElement.Add(checkerboardElement);
        
            tileElement.Add(coreElement);

            SetTileElement(tileElement);
        }

        public override Color Value
        {
            get => _colorElement.style.backgroundColor.value;
            set
            {
                _colorElement.style.backgroundColor = value;

                var colorWithoutAlpha = value;
                colorWithoutAlpha.a = 1;
                _opaqueElement.style.unityBackgroundImageTintColor = colorWithoutAlpha;
            }
        }

        public Color Color
        {
            get => Value;
            set => Value = value;
        }
    }
}