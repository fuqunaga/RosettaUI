using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    internal static class AnimationCurveEditorUtility
    {

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
   }
}