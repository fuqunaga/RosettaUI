using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public static class KeyframeExtensions
    {
        public static PointMode GetPointMode(this Keyframe keyframe)
        {
            if (keyframe is { inTangent: 0f, outTangent: 0f }) return PointMode.Flat;
             
            return Mathf.Approximately(keyframe.inTangent, keyframe.outTangent)
                ? PointMode.Smooth 
                : PointMode.Broken;
        }
        
        public static void SetPointMode(this ref Keyframe keyframe, PointMode mode)
        {
            switch (mode)
            {
                case PointMode.Smooth:
                    keyframe.inTangent = keyframe.outTangent;
                    break;
                case PointMode.Flat:
                    keyframe.inTangent = 0f;
                    keyframe.outTangent = 0f;
                    break;
                case PointMode.Broken:
                default:
                    break;
            }
        }
        
        public static bool GetWeightedFrag(this Keyframe key, WeightedMode mode)
        {
            return key.weightedMode.HasFlag(mode);
        }
        
        public static void SetWeightedFrag(this ref Keyframe key, WeightedMode mode, bool value)
        {
            uint current = (uint) key.weightedMode;
            uint mask = (uint) mode;
            if (value) { current |= mask; }
            else current &= ~mask;
            key.weightedMode = (WeightedMode)current;
        }
        
        public static void ToggleWeightedFrag(this ref Keyframe key, WeightedMode mode)
        {
            key.SetWeightedFrag(mode, !key.weightedMode.HasFlag(mode));
        }
    }
}