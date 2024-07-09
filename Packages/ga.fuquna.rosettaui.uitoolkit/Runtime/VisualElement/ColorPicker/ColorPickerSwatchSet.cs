using System;
using UnityEngine;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatchSet : SwatchSetBase<Color, ColorPickerSwatch>
    {
        public ColorPickerSwatchSet(Action<Color> applyValueFunc) : base("Swatches", applyValueFunc)
        {
        }

        protected override string DataKeyLayout => "ColorPickerSwatchSetLayout";
        protected override string DataKeyIsOpen => "ColorPickerSwatchSetIsOpen";
        protected override string DataKeySwatches => "ColorPickerSwatches";
    }
}