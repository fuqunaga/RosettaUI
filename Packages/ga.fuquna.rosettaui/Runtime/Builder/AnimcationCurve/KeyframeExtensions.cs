using UnityEngine;

namespace RosettaUI.Builder
{
    public static class KeyframeExtensions
    {
        public enum InOrOut
        {
            In,
            Out
        }
        
        public static InOrOut Opposite(this InOrOut inOrOut)
        {
            return inOrOut == InOrOut.In ? InOrOut.Out : InOrOut.In;
        }

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
        
        public static void SetPosition(this ref Keyframe keyframe, Vector2 position)
        {
            keyframe.time = position.x;
            keyframe.value = position.y;
        }
        
        public static void SetTangent(this ref Keyframe keyframe, InOrOut inOrOut, float tangent)
        {
            if (inOrOut == InOrOut.In)
            {
                keyframe.inTangent = tangent;
            }
            else
            {
                keyframe.outTangent = tangent;
            }
        }
        
        public static float GetTangent(this Keyframe keyframe, InOrOut inOrOut)
        {
            return inOrOut == InOrOut.In ? keyframe.inTangent : keyframe.outTangent;
        }
        
        public static void SetWeight(this ref Keyframe keyframe, InOrOut inOrOut, float weight)
        {
            if (inOrOut == InOrOut.In)
            {
                keyframe.inWeight = weight;
            }
            else
            {
                keyframe.outWeight = weight;
            }
        }
        
        public static float GetWeight(this Keyframe keyframe, InOrOut inOrOut)
        {
            return inOrOut == InOrOut.In ? keyframe.inWeight : keyframe.outWeight;
        }
        
        public static bool IsWeighted(this ref Keyframe keyframe, InOrOut inOrOut)
        {
            return keyframe.GetWeightedFrag(inOrOut == InOrOut.In ? WeightedMode.In : WeightedMode.Out);
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