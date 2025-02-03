using UnityEngine;

namespace RosettaUI.UIToolkit
{
    internal static class AnimationCurveEditorUtility
    {
        public static float GetOutWeight(this Keyframe keyframe)
        {
            return keyframe.weightedMode is WeightedMode.Out or WeightedMode.Both ? keyframe.outWeight : 1f / 3f;
        }

        public static float GetInWeight(this Keyframe keyframe)
        {
            return keyframe.weightedMode is WeightedMode.In or WeightedMode.Both ? keyframe.inWeight : 1f / 3f;
        }
        
        public static AnimationCurveEditorControlPoint.TangentMode GetTangentMode(this Keyframe keyframe)
        {
             if (keyframe is { inTangent: 0f, outTangent: 0f }) return AnimationCurveEditorControlPoint.TangentMode.Flat;
             if (Mathf.Approximately(keyframe.inTangent, keyframe.outTangent)) return AnimationCurveEditorControlPoint.TangentMode.Smooth;
             return AnimationCurveEditorControlPoint.TangentMode.Broken;
        }
        
        public static void SetTangentMode(ref Keyframe keyframe, AnimationCurveEditorControlPoint.TangentMode mode)
        {
            switch (mode)
            {
                case AnimationCurveEditorControlPoint.TangentMode.Smooth:
                    keyframe.inTangent = keyframe.outTangent;
                    break;
                case AnimationCurveEditorControlPoint.TangentMode.Flat:
                    keyframe.inTangent = 0f;
                    keyframe.outTangent = 0f;
                    break;
            }
        }
        
        public static float GetDegreeFromTangent(float tangent)
        {
            return Mathf.Atan(tangent) * Mathf.Rad2Deg;
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
        
        public static bool GetKey(KeyCode key)
        {
#if UNITY_INPUT_SYSTEM_ENABLED
        return Keyboard.current != null && Keyboard.current[(Key)key].isPressed;
#else
            return Input.GetKey(key);
#endif
        }

        public static bool GetKeyDown(KeyCode key)
        {
#if UNITY_INPUT_SYSTEM_ENABLED
        return Keyboard.current != null && Keyboard.current[(Key)key].wasPressedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

        public static bool GetKeyUp(KeyCode key)
        {
#if UNITY_INPUT_SYSTEM_ENABLED
        return Keyboard.current != null && Keyboard.current[(Key)key].wasReleasedThisFrame;
#else
            return Input.GetKeyUp(key);
#endif
        }
    }
}