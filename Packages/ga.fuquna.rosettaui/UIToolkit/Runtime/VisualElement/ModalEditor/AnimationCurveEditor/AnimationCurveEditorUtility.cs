using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    internal static class AnimationCurveEditorUtility
    {
        public static bool GetKeyBroken(this AnimationCurve curve, int index)
        {
#if UNITY_EDITOR
            return AnimationUtility.GetKeyBroken(curve, index);
#else
            return PredictGetBroken(curve, index);
#endif
        }

        public static TangentMode GetInTangentMode(this AnimationCurve curve, int index)
        {
#if UNITY_EDITOR
            return ToTangentMode(AnimationUtility.GetKeyLeftTangentMode(curve, index));
#else
            return PredictLeftTangentMode(curve, index);
#endif
        }

        public static TangentMode GetOutTangentMode(this AnimationCurve curve, int index)
        {
#if UNITY_EDITOR
            return ToTangentMode(AnimationUtility.GetKeyRightTangentMode(curve, index));
#else
            return PredictRightTangentMode(curve, index);
#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// KeyBrokenを設定する
        /// Editor中のみ有効。インスペクターと同期をとるために使用する。
        /// </summary>
        public static void SetKeyBroken(this AnimationCurve curve, int index, bool broken)
        {
            AnimationUtility.SetKeyBroken(curve, index, broken);
        }
        
        /// <summary>
        /// TangentModeを設定する
        /// Editor中のみ有効。インスペクターと同期をとるために使用する。
        /// </summary>
        public static void SetTangentMode(this AnimationCurve curve, int index, TangentMode leftMode, TangentMode rightMode)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, index, ToAnimationUtilityTangentMode(leftMode));
            AnimationUtility.SetKeyRightTangentMode(curve, index, ToAnimationUtilityTangentMode(rightMode));
        }
#endif
        

#if UNITY_EDITOR
        private static TangentMode ToTangentMode(AnimationUtility.TangentMode mode)
        {
            return mode switch
            {
                AnimationUtility.TangentMode.Free => TangentMode.Free,
                AnimationUtility.TangentMode.Linear => TangentMode.Linear,
                AnimationUtility.TangentMode.Constant => TangentMode.Constant,
                AnimationUtility.TangentMode.Auto => TangentMode.Free,
                AnimationUtility.TangentMode.ClampedAuto => TangentMode.Free,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
        
        private static AnimationUtility.TangentMode ToAnimationUtilityTangentMode(TangentMode mode)
        {
            return mode switch
            {
                TangentMode.Free => AnimationUtility.TangentMode.Free,
                TangentMode.Linear => AnimationUtility.TangentMode.Linear,
                TangentMode.Constant => AnimationUtility.TangentMode.Constant,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
#endif

        public static bool PredictGetBroken(this AnimationCurve curve, int index)
        {
            var keyframe = curve[index];
            return !Mathf.Approximately(keyframe.inTangent, keyframe.outTangent);
        }

        public static TangentMode PredictLeftTangentMode(this AnimationCurve curve, int index)
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

        public static TangentMode PredictRightTangentMode(this AnimationCurve curve, int index)
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
   }
}