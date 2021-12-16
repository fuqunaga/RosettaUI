using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class ClampFreeSlider : Slider
    {
        public ClampFreeSlider()
        {
            clamped = false;
        }

        internal override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
            => Mathf.Clamp01(base.SliderNormalizeValue(currentValue, lowerValue, higherValue));
    }
}
