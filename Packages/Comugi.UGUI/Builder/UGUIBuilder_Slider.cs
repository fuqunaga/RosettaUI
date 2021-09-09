using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_IntSlider(Element element)
        {
            var sliderElement = (IntSlider)element;
            var go = Build_InputField(sliderElement, resource.slider, TMP_InputField.ContentType.IntegerNumber, TryParseInt, out var inputFieldUI);

            var slider = go.GetComponentInChildren<Slider>();
            SetSliderColor(slider);

            var (min, max) = sliderElement.GetInitialMinMax();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = true;
            SetInputFieldTextToSlider(inputFieldUI.text, slider);

            slider.onValueChanged.AddListener((f) =>
            {
                inputFieldUI.SetTextWithoutNotify(f.ToString());
                sliderElement.OnViewValueChanged(Mathf.RoundToInt(f));
            });

            if (!sliderElement.IsMinMaxConst)
            {
                sliderElement.RegisterSetMinMaxToView((pair) => { slider.minValue = pair.Item1; slider.maxValue = pair.Item2; });
            }

            inputFieldUI.onValueChanged.AddListener((s) => SetInputFieldTextToSlider(s, slider));

            return go;


            void SetInputFieldTextToSlider(string text, Slider sliderComponent)
            {
                sliderComponent.SetValueWithoutNotify(TryParseInt(text).Item2);
            }
        }

        static GameObject Build_FloatSlider(Element element) => Build_FloatSliderBase((FloatSliderElement)element, (f) => f, (f) => f);


        static GameObject Build_LogSlider(Element element)
        {
            var slider = (LogSlider)element;
            var logBase = slider.logBase;

            return Build_FloatSliderBase(
                slider,
                (sliderValue) => Mathf.Pow(logBase, sliderValue),
                (f) => Mathf.Log(f, logBase)
                );
        }


        static GameObject Build_FloatSliderBase(Slider<float> sliderElement, Func<float, float> sliderToField, Func<float, float> fieldToSlider)
        {
            var go = Build_InputField(sliderElement, resource.slider, TMP_InputField.ContentType.DecimalNumber, TryParseFloat, out var inputFieldUI);

            var slider = go.GetComponentInChildren<Slider>();
            SetSliderColor(slider);

            var (min, max) = sliderElement.GetInitialMinMax();
            slider.minValue = fieldToSlider(min);
            slider.maxValue = fieldToSlider(max);
            slider.wholeNumbers = false;
            slider.value = fieldToSlider(sliderElement.GetInitialValue());

            slider.onValueChanged.AddListener((sliderValue) =>
            {
                var f = sliderToField(sliderValue);
                inputFieldUI.SetTextWithoutNotify(f.ToString());
                sliderElement.OnViewValueChanged(f);
            });

            if (!sliderElement.IsMinMaxConst)
            {
                sliderElement.RegisterSetMinMaxToView((pair) =>
                {
                    slider.minValue = fieldToSlider(pair.Item1);
                    slider.maxValue = fieldToSlider(pair.Item2);
                });
            }

            inputFieldUI.onValueChanged.AddListener((s) =>
            {
                var sliderValue = fieldToSlider(TryParseFloat(s).Item2);
                slider.SetValueWithoutNotify(sliderValue);
            });

            return go;
        }

        static void SetSliderColor(Slider slider)
        {
            var theme = settings.theme;
            slider.colors = theme.sliderColors;
            var bacground = slider.transform.Find("Background")?.GetComponent<Image>();
            if ( bacground != null)
            {
                bacground.color = theme.sliderBackgroundColor;
            }

            slider.fillRect.GetComponent<Image>().color = theme.sliderFillColor;
        }

    }
}