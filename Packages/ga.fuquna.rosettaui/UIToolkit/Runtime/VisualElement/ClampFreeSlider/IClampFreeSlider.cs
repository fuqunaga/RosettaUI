using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public interface IClampFreeSlider<TValue>
        where TValue : IComparable<TValue>
    {
        BaseSlider<TValue> Slider { get; }
        VisualElement InputField { get; }
    }

    
    public static class SliderInFieldExtension
    {
        private const string UssClassName = "rosettaui-slider-in-field";
        
        public static void Initialize<TClampFreeSlider, TValue>(this TClampFreeSlider sliderInField) 
            where TClampFreeSlider : BaseField<TValue>, IClampFreeSlider<TValue>
            where TValue : IComparable<TValue>
        {
            sliderInField.AddToClassList(UssClassName);
            
            var slider = sliderInField.Slider;
            slider.RegisterValueChangedCallback(evt => sliderInField.value = evt.newValue);
            
            sliderInField.Insert(0, slider);
        }
    }
}