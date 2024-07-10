using System;
using UnityEngine;

namespace RosettaUI.UIToolkit
{
    public class GradientEditorPresetSet : SwatchSetBase<Gradient, GradientEditorPreset>
    {
        public const string KeyPrefix = "RosettaUI-GradientEditorPresetSet";
        
        public GradientEditorPresetSet(Action<Gradient> applyValueFunc) : base("Preset", applyValueFunc)
        {
        }

        protected override string DataKeyPrefix => KeyPrefix;
    }
}