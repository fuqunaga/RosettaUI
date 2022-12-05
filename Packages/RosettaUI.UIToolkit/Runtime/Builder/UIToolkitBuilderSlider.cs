using System;
using RosettaUI.Reactive;
using UnityEngine;
using UnityEngine.UIElements;

#if !UNITY_2022_1_OR_NEWER
using RosettaUI.UIToolkit.UnityInternalAccess;
#endif

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_Slider<T, TSlider>(Element element)
            where T : IComparable<T>
            where TSlider : BaseSlider<T>, new()
        {
            var sliderElement = (SliderElement<T>) element;
            var slider = Build_Field<T, TSlider>(sliderElement);

            InitRangeFieldElement(sliderElement,
                (min) => slider.lowValue = min,
                (max) => slider.highValue = max
            );

            slider.showInputField = sliderElement.showInputField;
            return slider;
        }

        VisualElement Build_MinMaxSlider_Int(Element element) =>
            Build_MinMaxSlider<int, IntegerField>(element, (i) => i, (f) => (int) f);

        VisualElement Build_MinMaxSlider_Float(Element element) =>
            Build_MinMaxSlider<float, FloatField>(element, (f) => f, (f) => f);

        private VisualElement Build_MinMaxSlider<T, TTextField>(Element element, Func<T, float> toFloat, Func<float, T> toValue)
            where TTextField : TextInputBaseField<T>, new()
        {
            if (toValue == null) throw new ArgumentNullException(nameof(toValue));
            
            var sliderElement = (MinMaxSliderElement<T>) element;
            
            var slider = new MinMaxSlider();
            SetupFieldLabel(slider, sliderElement);


            TTextField minTextField = null;
            TTextField maxTextField = null;
            if (sliderElement.showInputField)
            {
                minTextField = new TTextField();
                maxTextField = new TTextField();

                minTextField.RegisterValueChangedCallback((evt) => slider.minValue = toFloat(evt.newValue));
                maxTextField.RegisterValueChangedCallback((evt) => slider.maxValue = toFloat(evt.newValue));
            }
            

            sliderElement.GetViewBridge().SubscribeValueOnUpdateCallOnce(minMax =>
                {
                    var min = toFloat(minMax.min);
                    var max = toFloat(minMax.max);

                    slider.SetValueWithoutNotifyIfNotEqual(new Vector2(min, max));
                    UpdateMinMaxTextField(minMax.min, minMax.max);
                }
            );

            slider.RegisterValueChangedCallback(evt =>
            {
                var vec2 = evt.newValue;
                var min = toValue(vec2.x);
                var max = toValue(vec2.y);
                
                sliderElement.GetViewBridge().SetValueFromView(MinMax.Create(min, max));
                UpdateMinMaxTextField(min, max);
            });

            InitRangeFieldElement(sliderElement,
                (min) => slider.lowLimit = toFloat(min),
                (max) => slider.highLimit = toFloat(max)
            );

            var row = new Row();
            row.AddToClassList(UssClassName.MinMaxSlider);
            row.Add(slider);
            row.Add(minTextField);
            row.Add(maxTextField);

            return row;

            
            void UpdateMinMaxTextField(T min, T max)
            {
                if (sliderElement.showInputField)
                {
                    minTextField.SetValueWithoutNotifyIfNotEqual(min);
                    maxTextField.SetValueWithoutNotifyIfNotEqual(max);
                }
            }
        }


        static void InitRangeFieldElement<T, TRange>(
            RangeFieldElement<T, TRange> rangeFieldElement,
            Action<TRange> updateMin,
            Action<TRange> updateMax
        )
        {
            if (rangeFieldElement.IsMinConst)
                updateMin(rangeFieldElement.Min);
            else
                rangeFieldElement.minRx.SubscribeAndCallOnce(updateMin);

            if (rangeFieldElement.IsMaxConst)
                updateMax(rangeFieldElement.Max);
            else
                rangeFieldElement.maxRx.SubscribeAndCallOnce(updateMax);
        }

    }
}