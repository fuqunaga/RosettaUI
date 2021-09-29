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

            _colorPicker.PrevColor = initialColor;
            _colorPicker.onColorChanged += onColorChanged;
            _colorPicker.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            _window.Show(position, target);

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

        private Vector3 _hsv;
        private float _alpha;

        public Color PrevColor
        {
            get => _previewPrev.style.backgroundColor.value;
            private set
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

                OnUpdateColor();
            }
        }


        private Vector3 Hsv
        {
            get => _hsv;
            set
            {
                if (_hsv == value) return;

                _hsv = value;
                OnUpdateColor();
            }
        }

        private ColorPicker()
        {
            _visualTreeAsset ??=Resources.Load<VisualTreeAsset>("RosettaUI_ColorPicker"); 
            _visualTreeAsset.CloneTree(this);
            
            InitPreview();
            InitHsvHandlers();
            InitSliders();
        }


        private void InitHsvHandlers()
        {
            if (_svTexture == null)
            {
                var size = ColorPickerHelper.defaultSvTextureSize;
                _svTexture = new Texture2D(size.x, size.y);
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

        private void InitSliders()
        {
            InitSlider("slider0", "H", 360, (v) =>
            {
                var hsv = Hsv;
                hsv.x = v / 360f;
                Hsv = hsv;
            });

            InitSlider("slider1", "S", 100, (v) =>
            {
                var hsv = Hsv;
                hsv.y = v / 100f;
                Hsv = hsv;
            });
        }

        void InitSlider(string name, string label, int max, Action<int> onSliderValueChanged)
        {
            var container = this.Q(name);
            //container.Q<Label>("label").text = label;
            var slider = container.Q<SliderInt>("slider");
            slider.label = label;
            slider.highValue = max;
            slider.RegisterValueChangedCallback((evt) => onSliderValueChanged(evt.newValue));
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


        void OnUpdateColor()
        {
            _previewCurr.style.backgroundColor = Color;
            
            var hsv = Hsv;
            ColorPickerHelper.UpdateSvTexture(_svTexture, hsv.x);

            var svCircleStyle = _svCircle.style;
            svCircleStyle.left = Length.Percent(hsv.y * 100f);
            svCircleStyle.top = Length.Percent((1f - hsv.z) * 100f);

            var hueCircleStyle = _hueCircle.style;
            hueCircleStyle.top = Length.Percent((1f - hsv.x) * 100f);

            onColorChanged?.Invoke(Color);
        }
    }
}