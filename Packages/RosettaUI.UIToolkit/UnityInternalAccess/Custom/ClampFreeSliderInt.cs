using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class ClampFreeSliderInt : SliderInt
    {
        public ClampFreeSliderInt()
        {
            clamped = false;
            SliderPatchUtility.CreateTextInputFieldAndBlockSliderKeyDownEvent(this);
        }
        internal override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
            => Mathf.Clamp01(base.SliderNormalizeValue(currentValue, lowerValue, higherValue));
    }
}