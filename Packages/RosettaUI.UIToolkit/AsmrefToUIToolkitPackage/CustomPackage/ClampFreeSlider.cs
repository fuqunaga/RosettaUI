using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.PackageInternal
{
    public class ClampFreeSlider : Slider
    {
        public ClampFreeSlider()
        {
            clamped = false;
        }
        
        internal override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
        {
            return Mathf.Clamp01((currentValue - lowerValue) / (higherValue - lowerValue));
        }

    }
}
