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

        public static void Show(Vector2 position, VisualElement target, Color initialColor, Action<Color> onClose)
        {
            if (_window == null)
            {
                _window = new ModalWindow();
                _colorPicker = new ColorPicker();
                _window.Add(_colorPicker);
            }

            _colorPicker.PrevColor = initialColor;
            _colorPicker.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            _window.Show(position, target);

            void OnDetach(DetachFromPanelEvent _)
            {
                onClose(_colorPicker.Color);
                _colorPicker.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            }
        }

        #endregion

        #region static members

        private static readonly string ussClassName = "rosettaui-colorpicker";
        private static readonly string ussClassNamePreview = ussClassName + "__preview";
        private static readonly string ussClassNamePreviewPrev = ussClassNamePreview + "-prev";
        private static readonly string ussClassNamePreviewCurrent = ussClassNamePreview + "-curr";
        private static readonly string ussClassNameHandler = ussClassName + "__handler";
        private static readonly string ussClassNameHandlerSV = ussClassNameHandler + "-sv";
        private static readonly string ussClassNameHandlerH = ussClassNameHandler + "-h";
        private static readonly string ussClassNameCircle = ussClassName + "__circle";

        private static Texture2D _svTexture;

        #endregion
        

        private readonly VisualElement _previewPrev;
        private readonly VisualElement _previewCurr;
        private readonly VisualElement _svHandler;
        private readonly VisualElement _svCircle;
        
        private Vector3 _hsv;
        private float _alpha;
        
        public Color PrevColor
        {
            get => _previewPrev.style.backgroundColor.value;
            private set
            {
                _previewPrev.style.backgroundColor = value;
                
                Color.RGBToHSV(value, out var h, out var s, out var v);
                _hsv = new Vector3(h, s, v);
                _alpha = value.a;

                OnUpdateColor();
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
            if (_svTexture == null)
            {
                _svTexture = new Texture2D(400, 400);
            }

            AddToClassList(ussClassName);

            var preview = CreateElement(ussClassNamePreview, this);
            _previewPrev = CreateElement(ussClassNamePreviewPrev, preview);
            _previewCurr = CreateElement(ussClassNamePreviewCurrent, preview);

            preview.style.backgroundImage = ColorPickerHelper.CheckerBoardTexture;

            var handler = CreateElement(ussClassNameHandler, this);
            _svHandler = CreateElement(ussClassNameHandlerSV, handler);
            _svCircle = CreateElement(ussClassNameCircle, _svHandler);
            CreateElement(ussClassNameHandlerH, handler);

            _svHandler.style.backgroundImage = _svTexture;
            _svHandler.RegisterCallback<PointerDownEvent>(OnPointDownOnSV);

            static VisualElement CreateElement(string className, VisualElement parent)
            {
                var element = new VisualElement();
                element.AddToClassList(className);
                parent.Add(element);

                return element;
            }
        }

        private void OnPointDownOnSV(PointerDownEvent evt)
        {
            var root = panel.visualTree;
            root.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnPanel);
            root.RegisterCallback<PointerUpEvent>(OnPointerUpOnPanel);

            evt.StopPropagation();
        }

        private void OnPointerUpOnPanel(PointerUpEvent evt)
        {
            var root = panel.visualTree;
            root.UnregisterCallback<PointerMoveEvent>(OnPointerMoveOnPanel);
            root.UnregisterCallback<PointerUpEvent>(OnPointerUpOnPanel);
            evt.StopPropagation();
        }

        private void OnPointerMoveOnPanel(PointerMoveEvent evt)
        {
            var localPos = _svHandler.WorldToLocal(evt.position);

            var svLayout = _svHandler.layout;
            var size = new Vector2(svLayout.width, svLayout.height);
            var sv = localPos / size;

            var hsv = Hsv;
            hsv.y = Mathf.Clamp01(sv.x);
            hsv.z = 1f - Mathf.Clamp01(sv.y);
            Hsv = hsv;

            evt.StopPropagation();
        }

        void OnUpdateColor()
        {
            _previewCurr.style.backgroundColor = Color;

            var hsv = Hsv;
            ColorPickerHelper.UpdateSVTexture(_svTexture, hsv.x);

            var svCircleStyle = _svCircle.style;
            svCircleStyle.left = Length.Percent(hsv.y * 100f);
            svCircleStyle.top = Length.Percent((1f - hsv.z) * 100f);
        }
    }
}