using System;
using UnityEngine;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatchSet : SwatchSetBase<Color, ColorPickerSwatch>
    {
        public new const string UssClassName = "rosettaui-colorpicker-swatchset";
        public const string KeyPrefix = "RosettaUI-ColorPickerSwatchSet";
        
        public ColorPickerSwatchSet(Action<Color> applyValueFunc) : base("Swatches", applyValueFunc)
        {
            AddToClassList(UssClassName);
        }

        protected override string DataKeyPrefix => KeyPrefix;
    }
}