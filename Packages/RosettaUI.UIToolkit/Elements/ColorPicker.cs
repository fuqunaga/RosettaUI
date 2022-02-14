using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPicker : VisualElement
    {
        #region static interface

        private static ModalWindow _window;
        private static ColorPicker _colorPicker;

        public static void Show(Vector2 position, VisualElement target, Color initialColor,
            Action<Color> onColorChanged)
        {
            if (_window == null)
            {
                _window = new ModalWindow();
                _colorPicker = new ColorPicker();
                _window.Add(_colorPicker);
            }

            _window.Show(position, target);

            // Show()前はPanelが設定されていないのでコールバック系はShow()
            _colorPicker.PrevColor = initialColor;
            _colorPicker.onColorChanged += onColorChanged;
            _colorPicker.RegisterCallback<DetachFromPanelEvent>(OnDetach);


            void OnDetach(DetachFromPanelEvent _)
            {
                _colorPicker.onColorChanged -= onColorChanged;
                _colorPicker.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            }
        }

        #endregion

        #region static members

        private static VisualTreeAsset _visualTreeAsset;
        private static Texture2D _svTexture;

        #endregion

        public event Action<Color> onColorChanged;

        private VisualElement _previewPrev;
        private VisualElement _previewCurr;
        private VisualElement _svHandler;
        private VisualElement _svCircle;
        private VisualElement _hueHandler;
        private VisualElement _hueCircle;

        private readonly SliderSet _sliderSet;


        private Vector3 _hsv;
        private float _alpha;

        private Color PrevColor
        {
            get => _previewPrev.style.backgroundColor.value;
            set
            {
                _previewPrev.style.backgroundColor = value;
                Color = value;
            }
        }

        public Color Color
        {
            get
            {
                var hsv = Hsv;
                var rgb = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
                return new Color(rgb.r, rgb.g, rgb.b, _alpha);
            }
            private set
            {
                Color.RGBToHSV(value, out var h, out var s, out var v);
                _hsv = new Vector3(h, s, v);
                _alpha = value.a;

                OnColorChanged();
            }
        }


        private Vector3 Hsv
        {
            get => _hsv;
            set
            {
                if (_hsv == value) return;

                _hsv = value;
                OnColorChanged();
            }
        }

        private float Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha == value) return;
                _alpha = value;
                OnColorChanged();
            }
        }

        private ColorPicker()
        {
            _visualTreeAsset ??= Resources.Load<VisualTreeAsset>("RosettaUI_ColorPicker");
            _visualTreeAsset.CloneTree(this);

            InitPreview();
            InitHsvHandlers();
            _sliderSet = new SliderSet(this);
        }


        private void InitHsvHandlers()
        {
            if (_svTexture == null)
            {
                var size = ColorPickerHelper.defaultSvTextureSize;
                _svTexture = ColorPickerHelper.CreateTexture(size.x, size.y);
            }

            _svHandler = this.Q("handler-sv");
            _svCircle = _svHandler.Q("circle");
            _hueHandler = this.Q("handler-h");
            _hueCircle = _hueHandler.Q("circle");

            _svHandler.style.backgroundImage = _svTexture;
            _hueHandler.style.backgroundImage = ColorPickerHelper.HueTexture;
            _hueCircle.style.left = Length.Percent(50f);

            PointerDrag.RegisterCallback(_svHandler, OnPointerMoveOnPanel_SV);
            PointerDrag.RegisterCallback(_hueHandler, OnPointerMoveOnPanel_Hue);
        }

        private void InitPreview()
        {
            var preview = this.Q("preview");
            _previewPrev = this.Q("preview-prev");
            _previewCurr = this.Q("preview-curr");

            preview.style.backgroundImage = ColorPickerHelper.CheckerBoardTexture;
            _previewPrev.RegisterCallback<PointerDownEvent>((_) => Color = PrevColor);
        }

        private void OnPointerMoveOnPanel_SV(PointerMoveEvent evt)
        {
            var localPos = _svHandler.WorldToLocal(evt.position);

            var svLayout = _svHandler.layout;
            var size = new Vector2(svLayout.width, svLayout.height);
            var xyRate = localPos / size;

            var hsv = Hsv;
            hsv.y = Mathf.Clamp01(xyRate.x);
            hsv.z = 1f - Mathf.Clamp01(xyRate.y);
            Hsv = hsv;

            evt.StopPropagation();
        }

        private void OnPointerMoveOnPanel_Hue(PointerMoveEvent evt)
        {
            var localPos = _hueHandler.WorldToLocal(evt.position);

            var hueLayout = _hueHandler.layout;
            var yRate = localPos.y / hueLayout.height;

            var hsv = Hsv;
            hsv.x = 1f - Mathf.Clamp01(yRate);
            Hsv = hsv;

            evt.StopPropagation();
        }


        void OnColorChanged()
        {
            _previewCurr.style.backgroundColor = Color;

            var hsv = Hsv;
            ColorPickerHelper.UpdateSvTexture(_svTexture, hsv.x);

            var svCircleStyle = _svCircle.style;
            svCircleStyle.left = Length.Percent(hsv.y * 100f);
            svCircleStyle.top = Length.Percent((1f - hsv.z) * 100f);

            var hueCircleStyle = _hueCircle.style;
            hueCircleStyle.top = Length.Percent((1f - hsv.x) * 100f);

            _sliderSet.OnColorChanged();

            onColorChanged?.Invoke(Color);
        }

        class SliderSet
        {
            #region Type Define

            enum SliderType
            {
                Hsv,
                Rgb
            }

            #endregion

            private readonly ColorPicker _colorPicker;

            private readonly Button _sliderTypeButton;
            private readonly Slider _slider0;
            private readonly Slider _slider1;
            private readonly Slider _slider2;
            private readonly Slider _slider3;

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

            public SliderSet(ColorPicker colorPicker)
            {
                _colorPicker = colorPicker;

                _sliderTypeButton = colorPicker.Q<Button>("slider-type-button");
                _sliderTypeButton.clicked += ToggleSliderType;

                _slider0 = InitSlider("slider0", 0);
                _slider1 = InitSlider("slider1", 1);
                _slider2 = InitSlider("slider2", 2);
                _slider3 = InitSlider("slider3", 3);

                SetSliderType(SliderType.Hsv);

                Slider InitSlider(string name, int idx)
                {
                    var ret = _colorPicker.Q<Slider>(name);
                    ret.highValue = 1f;
                    ret.RegisterValueChangedCallback((evt) => OnSliderValueChanged(evt.newValue, idx));
                    return ret;
                    
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
                            }
                        }
                    }
                }
            }

            public void OnColorChanged()
            {
                var vec3 = _sliderType switch
                {
                    SliderType.Hsv => Hsv,
                    SliderType.Rgb => (Vector3) (Vector4) Color,
                    _ => throw new ArgumentException()
                };

                _slider0.SetValueWithoutNotify(vec3.x);
                _slider1.SetValueWithoutNotify(vec3.y);
                _slider2.SetValueWithoutNotify(vec3.z);
                _slider3.SetValueWithoutNotify(Alpha);
            }


            private void ToggleSliderType()
            {
                var newType = _sliderType == SliderType.Hsv ? SliderType.Rgb : SliderType.Hsv;
                SetSliderType(newType);
            }

            void SetSliderType(SliderType sliderType)
            {
                _sliderType = sliderType;

                var buttonText = "";
                switch (sliderType)
                {
                    case SliderType.Hsv:
                        buttonText = "HSV";
                        SetSliderLabels("H", "S", "V", "A");
                        break;

                    case SliderType.Rgb:
                        buttonText = "RGB";
                        SetSliderLabels("R", "G", "B", "A");
                        break;
                }

                _sliderTypeButton.text = buttonText;

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