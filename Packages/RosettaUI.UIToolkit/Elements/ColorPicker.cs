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
        private VisualElement _hueCursor;

        private readonly SliderSet _sliderSet;

        private float _hueCircleThickness;

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
            _svHandler = this.Q("handler-sv");
            _svCircle = _svHandler.Q("circle");
            _hueHandler = this.Q("handler-h");
            _hueCursor = _hueHandler.Q("circle");
            
            PointerDrag.RegisterCallback(_svHandler, OnPointerMoveOnPanel_SV, CheckPointerIsValid_SV);
            PointerDrag.RegisterCallback(_hueHandler, OnPointerMoveOnPanel_Hue, CheckPointIsValid_Hue);

            
            schedule.Execute(() =>
            {
                // Hue Circle
                var hueCircleSize = _hueHandler.resolvedStyle.width;
                (_hueHandler.style.backgroundImage, _hueCircleThickness) = ColorPickerHelper.GetHueCircleTextureAndThickness(hueCircleSize);

                var cursorSize = _hueCircleThickness * 1.1f; // 画像内に影がついている関係で、画像のサイズ＝カーソルの直径ではない。ので見た目が合うように少し拡大しておく
                var cursorStyle = _hueCursor.style;
                cursorStyle.width = cursorSize;
                cursorStyle.height = cursorSize;
                cursorStyle.marginLeft = cursorSize * -0.5f;
                cursorStyle.marginTop = cursorSize * -0.5f;
                
                // SV Circle
                var svCircleSize = _svHandler.resolvedStyle.width; 
                
                if (_svTexture == null)
                {
                    var size = ColorPickerHelper.defaultSvTextureSize;
                    _svTexture = ColorPickerHelper.CreateTexture(size.x, size.y);
                }

                _svHandler.style.backgroundImage = _svTexture;
                
                
                // 表示更新
                OnColorChanged();
            });
        }

        private void InitPreview()
        {
            var preview = this.Q("preview");
            _previewPrev = this.Q("preview-prev");
            _previewCurr = this.Q("preview-curr");

            preview.style.backgroundImage = ColorPickerHelper.CheckerBoardTexture;
            _previewPrev.RegisterCallback<PointerDownEvent>((_) => Color = PrevColor);
        }


        bool CheckPointIsValid_Hue(PointerDownEvent evt)
        {
            var pos = evt.localPosition;
            
            var circleSize =_hueHandler.resolvedStyle.width;
            var center = Vector2.one * circleSize * 0.5f;
            var maxRadius = circleSize * 0.5f;
            
            var distance = Vector2.Distance(pos, center);
            
            return (maxRadius - _hueCircleThickness <= distance && distance <= maxRadius);
        }
        
        private void OnPointerMoveOnPanel_Hue(PointerMoveEvent evt)
        {
            var localPos = _hueHandler.WorldToLocal(evt.position);

            var hsv = Hsv;
            hsv.x = LocalPosToHue(localPos);
            Hsv = hsv;
            
            evt.StopPropagation();
        }

        bool CheckPointerIsValid_SV(PointerDownEvent evt)
        {
            var pos = evt.localPosition;

            var circleSize = _svHandler.resolvedStyle.width;
            var radius = circleSize * 0.5f;
            var center = Vector2.one * radius;
            
            var distance = Vector2.Distance(pos, center);

            return distance <= radius;
        }
        
        private void OnPointerMoveOnPanel_SV(PointerMoveEvent evt)
        {
            var localPos = _svHandler.WorldToLocal(evt.position);
            var radius = _svHandler.resolvedStyle.width * 0.5f;
            var posOnCircle = (localPos - Vector2.one * radius) / radius;

            var posOnSquare = ColorPickerHelper.CircleToSquare(posOnCircle);
            if (posOnSquare.sqrMagnitude > 1f)
            {
                posOnSquare.Normalize();
            }
            var sv = (posOnSquare + Vector2.one) * 0.5f; // map: -1~1 > 0~1

            var hsv = Hsv;
            hsv.y = sv.x;
            hsv.z = 1f - sv.y;
            Hsv = hsv;

            evt.StopPropagation();
        }

        void OnColorChanged()
        {
            _previewCurr.style.backgroundColor = Color;

            var hsv = Hsv;
            if (_svTexture != null)
            {
                ColorPickerHelper.UpdateSvCircleTexture(_svTexture, hsv.x);
            }

            var svCircleStyle = _svCircle.style;
            svCircleStyle.left = Length.Percent(hsv.y * 100f);
            svCircleStyle.top = Length.Percent((1f - hsv.z) * 100f);

            UpdateHueCursor(hsv.x);

            _sliderSet.OnColorChanged();

            onColorChanged?.Invoke(Color);
        }

        void UpdateHueCursor(float h)
        {
            var pos = HueToLocalPos(h);
            
            var s = _hueCursor.style;
            s.top = pos.y;
            s.left = pos.x;
        }


        Vector2 HueToLocalPos(float h)
        {
            var circleSize =_hueHandler.resolvedStyle.width; 
            var radius = (circleSize * 0.5f) - (_hueCircleThickness * 0.5f);
            
            var rad = h * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)) * radius
                      + (Vector2.one * circleSize * 0.5f);
        }

        float LocalPosToHue(Vector2 pos)
        {
            var circleSize =_hueHandler.resolvedStyle.width;

            var p = pos - (Vector2.one * circleSize * 0.5f);

            var f = Mathf.Atan2(-p.y, p.x) / (Mathf.PI * 2f);
            if (f < 0f) f += 1f;
            return f;
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