using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class ClampFreeSliderInt : SliderInt
    {
        public ClampFreeSliderInt()
        {
            //clamped = false;
        }

        /*
        internal override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
        {
            return Mathf.Clamp01((currentValue - (float) lowerValue) / (higherValue - (float) lowerValue));
        }
        */
    }
}