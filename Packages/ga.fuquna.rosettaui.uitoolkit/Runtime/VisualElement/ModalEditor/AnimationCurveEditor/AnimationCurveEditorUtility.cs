using RosettaUI.Builder;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    internal static class AnimationCurveEditorUtility
    {
        
        public static PointMode GetPointMode(this Keyframe keyframe)
        {
             if (keyframe is { inTangent: 0f, outTangent: 0f }) return PointMode.Flat;
             if (Mathf.Approximately(keyframe.inTangent, keyframe.outTangent)) return PointMode.Smooth;
             return PointMode.Broken;
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
        public static TangentMode GetInTangentMode(this AnimationCurve curve, int index)
        {
            var key = curve[index];
            
            if (float.IsPositiveInfinity(curve[index].inTangent)) return TangentMode.Constant;
            if (index > 0)
            {
                var prevKey = curve[index - 1];

                float tangentToPrevKey = (key.value - prevKey.value) / (key.time - prevKey.time);
                if (Mathf.Approximately(tangentToPrevKey, key.inTangent)) return TangentMode.Linear;
            }
            
            return TangentMode.Free;            
        }

        public static TangentMode GetOutTangentMode(this AnimationCurve curve, int index)
        {
            var key = curve[index];
            
            if (float.IsPositiveInfinity(curve[index].outTangent)) return TangentMode.Constant;
            if (index < curve.length - 1)
            {
                var nextKey = curve[index + 1];
                float tangentToNextKey = (nextKey.value - key.value) / (nextKey.time - key.time);
                if (Mathf.Approximately(tangentToNextKey, key.outTangent)) return TangentMode.Linear;
            } 
            
            return TangentMode.Free;
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
        
        public static float GetTangentFromDegree(float degree)
        {
            return degree switch
            {
                90f => float.PositiveInfinity,
                -90f => float.NegativeInfinity,
                _ => Mathf.Tan(degree * Mathf.Deg2Rad)
            };
        }
        
        public static float GetDegreeFromTangent2(float y, float x)
        {
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        public static void ApplyTangentMode(CurvePointContainer container)
        {
            var i = 0;
            foreach (var keyPoint in container)
            {
                var key = keyPoint.key;
                switch (keyPoint.point.InTangentMode)
                {
                    case TangentMode.Linear:
                        // Set the tangent to the slope between the previous key and this key
                        if (i > 0)
                        {
                            var prevKey = container[i - 1].key;
                            key.inTangent = (key.value - prevKey.value) / (key.time - prevKey.time);
                        }
                        keyPoint.point.SetPointMode(PointMode.Broken);
                        break;
                    case TangentMode.Constant:
                        key.inTangent = float.PositiveInfinity;
                        break;
                    case TangentMode.Free:
                    default:
                        break;
                }
                switch (keyPoint.point.OutTangentMode)
                {
                    case TangentMode.Linear:
                        // Set the tangent to the slope between this key and the next key
                        if (i < container.Count - 1)
                        {
                            var nextKey = container[i + 1].key;
                            key.outTangent = (nextKey.value - key.value) / (nextKey.time - key.time);
                        }
                        keyPoint.point.SetPointMode(PointMode.Broken);
                        break;
                    case TangentMode.Constant:
                        key.outTangent = float.PositiveInfinity;
                        break;
                    case TangentMode.Free:
                    default:
                        break;
                }
                
                container.MoveKey(i, key);
                i++;
            }
        }
        
    }
}