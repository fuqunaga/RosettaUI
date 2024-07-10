using System;
using UnityEngine;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatchSet : SwatchSetBase<Color, ColorPickerSwatch>
    {
        public const string KeyPrefix = "RosettaUI-ColorPickerSwatchSet";
        
        public ColorPickerSwatchSet(Action<Color> applyValueFunc) : base("Swatches", applyValueFunc)
        {
        }

        protected override string DataKeyPrefix => KeyPrefix;
    }
}