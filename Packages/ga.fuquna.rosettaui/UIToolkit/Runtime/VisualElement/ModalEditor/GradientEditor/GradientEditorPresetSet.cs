using System;
using UnityEngine;

namespace RosettaUI.UIToolkit
{
    public class GradientEditorPresetSet : SwatchSetFold<Gradient, GradientEditorPreset>
    {
        public const string KeyPrefix = "RosettaUI-GradientEditorPresetSet";
        
        public GradientEditorPresetSet(Action<Gradient> applyValueFunc) : base("Presets", applyValueFunc, KeyPrefix)
        {
        }
    }
}