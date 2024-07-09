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
        
        public static void SetShowInputField<TValue>(this IClampFreeSlider<TValue> clampFreeSlider,  bool showInputField)
            where TValue : IComparable<TValue>
        {
            clampFreeSlider.InputField.style.display = showInputField ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void Initialize<TClampFreeSlider, TValue>(this TClampFreeSlider sliderInField) 
            where TClampFreeSlider : BaseField<TValue>, IClampFreeSlider<TValue>
            where TValue : IComparable<TValue>
        {
            sliderInField.AddToClassList(UssClassName);
            
            var slider = sliderInField.Slider;
            slider.RegisterValueChangedCallback(evt => sliderInField.value = evt.newValue);
            
            // 矢印キーでスライダーを動かしたときにフォーカスも左右のエレメントに移動するのを抑制
            slider.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                if (evt.direction is NavigationMoveEvent.Direction.Left or NavigationMoveEvent.Direction.Right)
                {
                    evt.StopPropagationAndFocusControllerIgnoreEvent();
                }
            });
            
            sliderInField.Insert(0, slider);
        }
    }
}