using RosettaUI.Reactive;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_IntSlider(Element element)
        {
            var sliderElement = (IntSliderElement)element;
            var go = Build_InputField(sliderElement, resource.slider, TMP_InputField.ContentType.IntegerNumber, TryParseInt, out var inputFieldUI);

            var slider = go.GetComponentInChildren<Slider>();
            SetSliderColor(slider);
            
            slider.minValue = sliderElement.Min;
            slider.maxValue = sliderElement.Max;
            slider.wholeNumbers = true;
            SetInputFieldTextToSlider(inputFieldUI.text, slider);

            slider.onValueChanged.AddListener((f) =>
            {
                inputFieldUI.SetTextWithoutNotify(f.ToString(CultureInfo.InvariantCulture));
                sliderElement.GetViewBridge().SetValueFromView(Mathf.RoundToInt(f));
            });

            if (!sliderElement.IsMinConst)
            {
                sliderElement.minRx.Subscribe((min) => slider.minValue = min);
            }
            
            if (!sliderElement.IsMaxConst)
            {
                sliderElement.maxRx.Subscribe((max) => slider.maxValue = max);
            }

            inputFieldUI.onValueChanged.AddListener((s) => SetInputFieldTextToSlider(s, slider));

            return go;


            void SetInputFieldTextToSlider(string text, Slider sliderComponent)
            {
                sliderComponent.SetValueWithoutNotify(TryParseInt(text).Item2);
            }
        }

        static GameObject Build_FloatSlider(Element element) => Build_FloatSliderBase((FloatSliderElement)element, (f) => f, (f) => f);


        /*
        static GameObject Build_LogSlider(Element element)
        {
            var slider = (LogSliderElement)element;
            var logBase = slider.logBase;

            return Build_FloatSliderBase(
                slider,
                (sliderValue) => Mathf.Pow(logBase, sliderValue),
                (f) => Mathf.Log(f, logBase)
                );
        }
        */


        static GameObject Build_FloatSliderBase(SliderElement<float> sliderElement, Func<float, float> sliderToField, Func<float, float> fieldToSlider)
        {
            var go = Build_InputField(sliderElement, resource.slider, TMP_InputField.ContentType.DecimalNumber, TryParseFloat, out var inputFieldUI);

            var slider = go.GetComponentInChildren<Slider>();
            SetSliderColor(slider);

            slider.minValue = fieldToSlider(sliderElement.Min);
            slider.maxValue = fieldToSlider(sliderElement.Max);
            slider.wholeNumbers = false;
            slider.value = fieldToSlider(sliderElement.Value);

            slider.onValueChanged.AddListener((sliderValue) =>
            {
                var f = sliderToField(sliderValue);
                inputFieldUI.SetTextWithoutNotify(f.ToString(CultureInfo.InvariantCulture));
                sliderElement.GetViewBridge().SetValueFromView(f);
            });

            if (!sliderElement.IsMinConst)
            {
                sliderElement.minRx.Subscribe((min) => slider.minValue = fieldToSlider(min));
            }
            
            if (!sliderElement.IsMaxConst)
            {
                sliderElement.maxRx.Subscribe((max) => slider.maxValue = fieldToSlider(max));
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