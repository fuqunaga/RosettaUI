using System;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class AnimationCurveEditorPresetSet : SwatchSetFold<AnimationCurve, Preset>
    {
        public const string KeyPrefix = "RosettaUI-AnimationCurvePresetSet";
        
        public AnimationCurveEditorPresetSet(Action<AnimationCurve> applyValueFunc) : base("Presets", applyValueFunc, KeyPrefix)
        {
        }
    }
}