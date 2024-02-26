using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public partial class ColorPicker
    {
        private class SliderSet
        {
            private static readonly string USSClassNameTextureSlider = "rosettaui-texture-slider";

            private static Vector3 Round(Vector3 value, int digit)
            {
                var scale = Mathf.Pow(10f, digit);
                var v = value * scale;

                return new Vector3(
                    Mathf.Round(v.x),
                    Mathf.Round(v.y),
                    Mathf.Round(v.z)
                ) / scale;
            }

            private static float Round(float value, int digit)
            {
                var scale = Mathf.Pow(10f, digit);
                return Mathf.Round(value * scale) / scale;
            }

            #region Type Define

            private enum SliderType
            {
                Hsv,
                Rgb
            }

            #endregion

            private bool _isInitialized;

            private readonly ColorPicker _colorPicker;

            private readonly Button _sliderTypeButton;
            private readonly Slider _slider0;
            private readonly Slider _slider1;
            private readonly Slider _slider2;
            private readonly Slider _slider3;

            private Texture2D _sliderTexture0;
            private Texture2D _sliderTexture1;
            private Texture2D _sliderTexture2;
            
            private SliderType _sliderType;

            private Color Color
            {
                get => _colorPicker.Color;
                set => _colorPicker.Color = value;
            }

            private float Alpha
            {
                get => _colorPicker.Alpha;
                set => _colorPicker.Alpha = value;
            }

            private Vector3 Hsv
            {
                get => _colorPicker.Hsv;
                set => _colorPicker.Hsv = value;
            }

            public bool DisplayAlpha
            {
                get => _slider3.resolvedStyle.display != DisplayStyle.None;
                set => _slider3.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }

            public SliderSet(ColorPicker colorPicker)
            {
                _colorPicker = colorPicker;

                _sliderTypeButton = colorPicker.Q<Button>("slider-type-button");
                _sliderTypeButton.clicked += ToggleSliderType;

                _slider0 = InitSlider("slider0", 0, true);
                _slider1 = InitSlider("slider1", 1, true);
                _slider2 = InitSlider("slider2", 2, true);
                _slider3 = InitSlider("slider3", 3, false);

                _colorPicker.ScheduleToUseResolvedLayoutBeforeRendering(() =>
                {
                    _sliderTexture0 = InitSliderTexture(_slider0);
                    _sliderTexture1 = InitSliderTexture(_slider1);
                    _sliderTexture2 = InitSliderTexture(_slider2);

                    _isInitialized = true;
                    
                    SetSliderType(SliderType.Hsv);
                });
                return;

                Slider InitSlider(string name, int idx, bool isTextureSlider)
                {
                    var slider = _colorPicker.Q<Slider>(name);
                    slider.highValue = 1f;
                    slider.RegisterValueChangedCallback((evt) => OnSliderValueChanged(evt.newValue, idx));
                    
                    if (isTextureSlider)
                    {
                        slider.AddToClassList(USSClassNameTextureSlider);
                    }
                    
                    return slider;
                    
                    
                    void OnSliderValueChanged(float value, int index)
                    {
                        if (index == 3)
                        {
                            Alpha = value;
                        }
                        else
                        {
                            switch (_sliderType)
                            {
                                case SliderType.Hsv:
                                    var hsv = Hsv;
                                    hsv[index] = Mathf.Clamp01(value);
                                    Hsv = hsv;
                                    break;

                                case SliderType.Rgb:
                                    var color = Color;
                                    color[index] = Mathf.Clamp01(value);
                                    Color = color;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }

            private static Texture2D InitSliderTexture(VisualElement slider)
            {
                var tracker = slider.Q("unity-tracker");
                var tex = TextureUtility.CreateTexture(
                    Mathf.CeilToInt(tracker.resolvedStyle.width),
                    1,
                    TextureFormat.RGB24);

                tracker.style.backgroundImage = tex;

                return tex;
            }

            public void OnHsvChanged(bool hChanged, bool sChanged, bool vChanged)
            {
                if (!_isInitialized) return;
                
                switch (_sliderType)
                {
                    case SliderType.Hsv:
                        OnHsvChanged_Hsv(hChanged, sChanged, vChanged);
                        break;
                    case SliderType.Rgb:
                        OnHsvChanged_Rgb();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public void OnAlphaChanged()
            {
                _slider3.SetValueWithoutNotify(Round(Alpha, TextDigit));
            }

            private void OnHsvChanged_Hsv(bool hChanged, bool sChanged, bool vChanged)
            {
                Set3SliderValuesWithoutNotify(Hsv);

                if (hChanged || vChanged)
                {
                    ColorPickerHelper.UpdateSSliderTexture(_sliderTexture1, Hsv.x, Hsv.z);
                }

                if (hChanged || sChanged)
                {
                    ColorPickerHelper.UpdateVSliderTexture(_sliderTexture2, Hsv.x, Hsv.y);
                }
            }

            private void OnHsvChanged_Rgb()
            {
                var color = Color;
                Set3SliderValuesWithoutNotify((Vector4)color);
                
                ColorPickerHelper.UpdateRSliderTexture(_sliderTexture0, color);
                ColorPickerHelper.UpdateGSliderTexture(_sliderTexture1, color);
                ColorPickerHelper.UpdateBSliderTexture(_sliderTexture2, color);
            }

            private void Set3SliderValuesWithoutNotify(Vector3 value)
            {
                var v = Round(value, TextDigit); // 数値表示が枠に収まるように端数を丸める

                _slider0.SetValueWithoutNotify(v.x);
                _slider1.SetValueWithoutNotify(v.y);
                _slider2.SetValueWithoutNotify(v.z);
            }

            private void UpdateViewAll()
            {
                OnHsvChanged(true, true, true);
                OnAlphaChanged();
            }


            private void ToggleSliderType()
            {
                var newType = _sliderType == SliderType.Hsv ? SliderType.Rgb : SliderType.Hsv;
                SetSliderType(newType);
            }

            private void SetSliderType(SliderType sliderType)
            {
                _sliderType = sliderType;

                var buttonText = "";
                switch (sliderType)
                {
                    case SliderType.Hsv:
                        buttonText = "HSV";
                        SetSliderLabels("H", "S", "V", "A");
                        ColorPickerHelper.WriteHueSliderTexture(_sliderTexture0);
                        break;

                    case SliderType.Rgb:
                        buttonText = "RGB";
                        SetSliderLabels("R", "G", "B", "A");
                        break;
                }

                _sliderTypeButton.text = buttonText;

                UpdateViewAll();

                void SetSliderLabels(string label0, string label1, string label2, string label3)
                {
                    _slider0.label = label0;
                    _slider1.label = label1;
                    _slider2.label = label2;
                    _slider3.label = label3;
                }
            }
        }

    }
}