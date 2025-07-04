using UnityEngine;

namespace RosettaUI.Builder
{
    public static class KeyframeExtension
    {
        public static Vector2 GetStartVel(this Keyframe keyframe)
        {
            float weight = keyframe.weightedMode is WeightedMode.Out or WeightedMode.Both ? keyframe.outWeight : 1f / 3f;
            var tangent = new Vector2(1, keyframe.outTangent);
            if (weight == 0f && float.IsInfinity(tangent.y)) return tangent;
            return weight * tangent;
        }

        public static Vector2 GetEndVel(this Keyframe keyframe)
        {
            float weight = keyframe.weightedMode is WeightedMode.In or WeightedMode.Both ? keyframe.inWeight : 1f / 3f;
            var tangent = new Vector2(1, keyframe.inTangent);
            if (weight == 0f && float.IsInfinity(tangent.y)) return tangent;
            return weight * tangent;
        }

        public static Vector2 GetPosition(this Keyframe keyframe)
        {
            return new Vector2(keyframe.time, keyframe.value);
        }
    }
}