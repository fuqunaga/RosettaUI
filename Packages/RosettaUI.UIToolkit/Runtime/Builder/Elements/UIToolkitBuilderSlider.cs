using System;
using RosettaUI.Reactive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Slider<TValue, TSlider>(Element element, VisualElement visualElement)
            where TValue : IComparable<TValue>
            where TSlider : BaseField<TValue>, IClampFreeSlider<TValue>, new()
        {
            if (element is not SliderElement<TValue> sliderElement || visualElement is not TSlider sliderInField) return false;

            sliderInField.SetShowInputField(sliderElement.showInputField);

            BindRangeFieldElement(sliderElement,
                (min) => sliderInField.Slider.lowValue = min,
                (max) => sliderInField.Slider.highValue = max
            );
            
            Bind_Field<TValue, TSlider>(sliderElement, sliderInField);

            return true;
        }

        private bool Bind_MinMaxSlider<TValue, TTextField>(Element element, VisualElement visualElement)
            where TTextField : TextValueField<TValue>, new()
        {
            if (element is not MinMaxSliderElement<TValue> sliderElement 
                || visualElement is not MinMaxSliderWithField<TValue, TTextField> slider) return false;
            
            Bind_FieldLabel(sliderElement,  slider);
            
            slider.ShowInputField = sliderElement.showInputField;
            
            
            sliderElement.Bind(slider,
                elementValueToFieldValue: minMax => new Vector2(ToFloat(minMax.min), ToFloat(minMax.max)),
                fieldValueToElementValue: vec2 => MinMax.Create(ToTValue(vec2.x), ToTValue(vec2.y))
                );
            
            var viewBridge = sliderElement.GetViewBridge();
            viewBridge.SubscribeValueOnUpdateCallOnce(minMax => slider.value = new Vector2(ToFloat(minMax.min), ToFloat(minMax.max)));
            slider.RegisterValueChangedCallback(OnValueChanged);
            viewBridge.onUnsubscribe +=  () => slider?.UnregisterValueChangedCallback(OnValueChanged);
            
            BindRangeFieldElement(sliderElement,
                (min) => slider.lowLimit = ToFloat(min),
                (max) => slider.highLimit = ToFloat(max)
            );
            

            return true;


            void OnValueChanged(ChangeEvent<Vector2> evt)
            {
                var minMax = MinMax.Create(
                    ToTValue(evt.newValue.x),
                    ToTValue(evt.newValue.y)
                    );
                
                viewBridge.SetValueFromView(minMax);
            }
            
            float ToFloat(TValue value) => MinMaxSliderWithField<TValue, TTextField>.ToFloat(value);
            TValue ToTValue(float floatValue) => MinMaxSliderWithField<TValue, TTextField>.ToTValue(floatValue);
        }


        private static void BindRangeFieldElement<T, TRange>(
            RangeFieldElement<T, TRange> rangeFieldElement,
            Action<TRange> updateMin,
            Action<TRange> updateMax
        )
        {
            if (rangeFieldElement.IsMinConst)
            {
                updateMin(rangeFieldElement.Min);
            }
            else
            {
                var disposable = rangeFieldElement.minRx.SubscribeAndCallOnce(updateMin);
                rangeFieldElement.GetViewBridge().onUnsubscribe += disposable.Dispose;
            }

            if (rangeFieldElement.IsMaxConst)
            {
                updateMax(rangeFieldElement.Max);
            }
            else
            {
                var disposable = rangeFieldElement.maxRx.SubscribeAndCallOnce(updateMax);
                rangeFieldElement.GetViewBridge().onUnsubscribe += disposable.Dispose;
            }
        }

    }
}