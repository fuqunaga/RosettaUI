using RosettaUI.Builder;
using UnityEngine;

namespace RosettaUI.Undo
{
    public static class UndoHelper
    {
        // To prevent the value used for Undo from being modified externally, make a copy
        // For reference types, such as ObjectField's Object, it's only relevant in the Editor, so it's ignored
        public static TValue Clone<TValue>(TValue value)
        {
            if (value is null)
            {
                return default;
            }
            
            return value switch
            {
                Gradient g => (TValue)(object)GradientHelper.Clone(g),
                AnimationCurve ac => (TValue)(object)AnimationCurveHelper.Clone(ac),
                _ => value
            };
        }
    }
}