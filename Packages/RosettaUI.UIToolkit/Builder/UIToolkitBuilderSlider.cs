using System;
using RosettaUI.Reactive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        

        private static VisualElement Build_Slider<T, TSlider>(Element element)
            where T : IComparable<T>
            where TSlider : BaseSlider<T>, new()
        {
            var sliderElement = (SliderElement<T>) element;
            var slider = Build_Field<T, TSlider>(sliderElement);

            InitRangeFieldElement(sliderElement,
                (min) => slider.lowValue = min,
                (max) => slider.highValue = max
            );

            slider.showInputField = true;
            return slider;
        }

        static VisualElement Build_MinMaxSlider_Int(Element element) =>
            Build_MinMaxSlider<int, IntegerField>(element, (i) => i, (f) => (int) f);

        static VisualElement Build_MinMaxSlider_Float(Element element) =>
            Build_MinMaxSlider<float, FloatField>(element, (f) => f, (f) => f);

        private static VisualElement Build_MinMaxSlider<T, TTextField>(Element element, Func<T, float> toFloat,
            Func<float, T> toValue)
            where TTextField : TextInputBaseField<T>, new()
        {
            if (toValue == null) throw new ArgumentNullException(nameof(toValue));

            var minTextField = new TTextField();
            var maxTextField = new TTextField();

            var sliderElement = (MinMaxSliderElement<T>) element;
            
            /*
            var slider = Build_Field<MinMax<T>, Vector2, MinMaxSlider>(
                sliderElement,
                onElementValueChanged: (field, minMax) =>
                {
                    var vec2 = new Vector2(toFloat(minMax.min), toFloat(minMax.max));

                    field.SetValueWithoutNotify(vec2);
                    minTextField.SetValueWithoutNotify(minMax.min);
                    maxTextField.SetValueWithoutNotify(minMax.max);
                },
                (vec2) => MinMax.Create(toValue(vec2.x), toValue(vec2.y))
            );
            */
            var slider = new MinMaxSlider();
            slider.Bind(sliderElement,
                elementValueToFieldValue: minMax => new Vector2(toFloat(minMax.min), toFloat(minMax.max)),
                fieldValueToElementValue: vec2 => MinMax.Create(toValue(vec2.x), toValue(vec2.y))
                );
            SetupLabelCallback(slider, sliderElement);
            

            InitRangeFieldElement(sliderElement,
                (min) => slider.lowLimit = toFloat(min),
                (max) => slider.highLimit = toFloat(max)
            );

            minTextField.RegisterValueChangedCallback((evt) => slider.minValue = toFloat(evt.newValue));
            maxTextField.RegisterValueChangedCallback((evt) => slider.maxValue = toFloat(evt.newValue));


            var row = CreateRowVisualElement();

            row.AddToClassList(UssClassName.MinMaxSlider);
            minTextField.AddToClassList(UssClassName.MinMaxSliderTextField);
            maxTextField.AddToClassList(UssClassName.MinMaxSliderTextField);

            row.Add(slider);
            row.Add(minTextField);
            row.Add(maxTextField);

            return row;
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