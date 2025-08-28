using System;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class AnimationCurveEditorPresetSet : SwatchSetBase<AnimationCurve, Preset>
    {
        public const string KeyPrefix = "RosettaUI-AnimationCurvePresetSet";
        
        public AnimationCurveEditorPresetSet(Action<AnimationCurve> applyValueFunc) : base("Presets", applyValueFunc, KeyPrefix)
        {
            AddToClassList(UssClassName);
        }
    }
}