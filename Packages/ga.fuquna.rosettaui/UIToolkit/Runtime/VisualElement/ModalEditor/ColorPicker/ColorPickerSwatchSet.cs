using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace RosettaUI.UIToolkit
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class ColorPickerSwatchSet : SwatchSetFold<Color, ColorPickerSwatch>
    {
        public const string UssClassName = "rosettaui-colorpicker-swatchset";
        public const string KeyPrefix = "RosettaUI-ColorPickerSwatchSet";
        
        public ColorPickerSwatchSet(Action<Color> applyValueFunc) : base("Swatches", applyValueFunc, KeyPrefix)
        {
            AddToClassList(UssClassName);
        }
    }
}