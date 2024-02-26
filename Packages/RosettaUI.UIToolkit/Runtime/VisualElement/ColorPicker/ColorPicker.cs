using System;
using System.Diagnostics.CodeAnalysis;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public partial class ColorPicker : VisualElement
    {
        #region static interface

        private static ModalWindow _window;
        private static ColorPicker _colorPickerInstance;
        public static int TextDigit { get; set; } = 3;

        public static void Show(Vector2 position, VisualElement target, Color initialColor,
            Action<Color> onColorChanged, bool enableAlpha = true)
        {
            if (_window == null)
            {
                _window = new ModalWindow();
                _colorPickerInstance = new ColorPicker();
                _window.Add(_colorPickerInstance);

                _window.RegisterCallback<NavigationSubmitEvent>(_ => _window.Hide());
                _window.RegisterCallback<NavigationCancelEvent>(_ =>
                {
                    onColorChanged?.Invoke(initialColor);
                    _window.Hide();
                });
            }

            _window.Show(position, target);

            // はみ出し抑制
            VisualElementExtension.CheckOutOfScreen(position, _window);
            
            // Show()前はPanelが設定されていないのでコールバック系はShow()後
            _colorPickerInstance.PrevColor = initialColor;
            _colorPickerInstance.EnableAlpha = enableAlpha;
            _colorPickerInstance.onColorChanged += onColorChanged;
            _colorPickerInstance.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            
            return;

            void OnDetach(DetachFromPanelEvent _)
            {
                _colorPickerInstance.onColorChanged -= onColorChanged;
                _colorPickerInstance.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
                
                target?.Focus();
            }
        }

        #endregion

        #region static members

        private static VisualTreeAsset _visualTreeAsset;
        private static RenderTexture _svTexture;

        #endregion

        public event Action<Color> onColorChanged;

        private VisualElement _previewPrev;
        private VisualElement _previewCurr;
        private VisualElement _hueHandler;
        private VisualElement _hueCursor;
        private VisualElement _svHandler;
        private VisualElement _svCursor;

        private readonly SliderSet _sliderSet;
        
        private TextField _hex;

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

        private bool EnableAlpha
        {
            get => _sliderSet.DisplayAlpha;
            set => _sliderSet.DisplayAlpha = value;
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
                if (Color != value)
                {
                    Color.RGBToHSV(value, out var h, out var s, out var v);
                    var oldHsv = Hsv;
                    _hsv = new Vector3(h, s, v);
                    _alpha = value.a;

                    OnHsvChanged(_hsv, oldHsv);
                    OnAlphaChanged();
                    OnColorChanged();
                }
            }
        }


        private Vector3 Hsv
        {
            get => _hsv;
            set
            {
                if (_hsv == value) return;

                var old = _hsv;
                _hsv = value;
                OnHsvChanged(_hsv, old);
                OnColorChanged();
            }
        }

        private float Alpha
        {
            get => _alpha;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_alpha == value) return;
                _alpha = value;
                OnAlphaChanged();
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
            InitHex();
        }

        private void InitPreview()
        {
            var preview = this.Q("preview");
            _previewPrev = this.Q("preview-prev");
            _previewCurr = this.Q("preview-curr");

            preview.style.backgroundImage = ColorPickerHelper.CheckerBoardTexture;
            _previewPrev.RegisterCallback<PointerDownEvent>((_) => Color = PrevColor);
        }

        private void InitHsvHandlers()
        {
            _hueHandler = this.Q("handler-h");
            _hueCursor = _hueHandler.Q("circle");
            _svHandler = this.Q("handler-sv");
            _svCursor = _svHandler.Q("circle");

            PointerDrag.RegisterCallback(_hueHandler, OnPointerMoveOnPanel_Hue, null, CheckPointIsValid_Hue, true);
            PointerDrag.RegisterCallback(_svHandler, OnPointerMoveOnPanel_SV, null, CheckPointerIsValid_SV, true);

            
            this.ScheduleToUseResolvedLayoutBeforeRendering(() =>
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
            
                // SV Disk
                var width = Mathf.CeilToInt(_svHandler.resolvedStyle.width);
                var height = Mathf.CeilToInt(_svHandler.resolvedStyle.height);
                _svTexture = ColorPickerHelper.CreateRenderTexture(width, height);
                _svHandler.style.backgroundImage = Background.FromRenderTexture(_svTexture);

                // 表示更新
                UpdateSvDisk();
                UpdateHueCursor(Hsv.x);
                UpdateSvCursor(Hsv.y, Hsv.z);
                    
                // はみ出し抑制
                VisualElementExtension.CheckOutOfScreen(_window.Position, _window);
            });
        }

        private void InitHex()
        {
            _hex = this.Q<TextField>("hex");
            
            _hex.RegisterValueChangedCallback(evt =>
            {
                var str = evt.newValue;
                if (ColorPickerHelper.HexToColor(str) is { } col32)
                {
                    Color col = col32;
                    col.a = Alpha;
                    Color = col;
                }
            });

            _hex.RegisterCallback<BlurEvent>(_ => UpdateHex()); // 変な文字列を修正。入力中は維持的に文字数が足りなかったりしてもいいので、フォーカスが外れる際に修正する
            
            UpdateHex();
        }

        private bool CheckPointIsValid_Hue(PointerDownEvent evt)
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

        private bool CheckPointerIsValid_SV(PointerDownEvent evt)
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
            if (posOnCircle.sqrMagnitude >= 1f)
            {
                posOnCircle.Normalize();
            }

            var posOnSquare = ColorPickerHelper.CircleToSquare(posOnCircle);
      
            var sv = (posOnSquare + Vector2.one) * 0.5f; // map: -1~1 > 0~1
            
            var hsv = Hsv;
            hsv.y = Mathf.Clamp01(sv.x);
            hsv.z = Mathf.Clamp01(1f - sv.y);　// 数値表示が枠に収まるように端数を丸める
            Hsv = hsv;

            evt.StopPropagation();
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void OnHsvChanged(Vector3 newValue, Vector3 oldValue)
        {
            var hChanged = newValue.x != oldValue.x;
            var sChanged = newValue.y != oldValue.y; 
            var vChanged = newValue.z != oldValue.z;
            
            if ( hChanged )
            {
                UpdateSvDisk();
                UpdateHueCursor(newValue.x);
            }


            if (sChanged || vChanged)
            {
                UpdateSvCursor(newValue.y, newValue.z);
            }
            
            _sliderSet.OnHsvChanged(hChanged, sChanged, vChanged);
        }

        private void OnAlphaChanged()
        {
            _sliderSet.OnAlphaChanged();
        }

        private void OnColorChanged()
        {
            _previewCurr.style.backgroundColor = Color;
            UpdateHex();
            onColorChanged?.Invoke(Color);
        }


        private void UpdateHueCursor(float h)
        {
            var pos = HueToLocalPos(h);
            
            var s = _hueCursor.style;
            s.top = pos.y;
            s.left = pos.x;
        }


        private Vector2 HueToLocalPos(float h)
        {
            var circleSize =_hueHandler.resolvedStyle.width; 
            var radius = (circleSize * 0.5f) - (_hueCircleThickness * 0.5f);
            
            var rad = h * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)) * radius
                      + (Vector2.one * circleSize * 0.5f);
        }

        private float LocalPosToHue(Vector2 pos)
        {
            var circleSize =_hueHandler.resolvedStyle.width;

            var p = pos - (Vector2.one * circleSize * 0.5f);

            var f = Mathf.Atan2(-p.y, p.x) / (Mathf.PI * 2f);
            if (f < 0f) f += 1f;
            return f;
        }


        private void UpdateSvCursor(float s, float v)
        {
            var xy = new Vector2(s, v) * 2f - Vector2.one; //map: 0~1 > -1~1

            var uv = ColorPickerHelper.SquareToCircle(xy);

            /*
            // カーソルを円に収めるために少しだけ縮める
            var cursorSize = _svCursor.resolvedStyle.width;
            var parentSize = _svHandler.resolvedStyle.width;
            uv *= (1f - cursorSize / parentSize);
            */ 
            
            var circleXY = (uv + Vector2.one) * 0.5f; // map: -1~1 > 0~1

            var cursorStyle = _svCursor.style;
            cursorStyle.left = Length.Percent(circleXY.x * 100f);
            cursorStyle.top = Length.Percent((1f - circleXY.y) * 100f);
        }

        private void UpdateSvDisk()
        {
            if (_svTexture != null)
            {
                ColorPickerHelper.UpdateSvDiskTexture(_svTexture, Hsv.x);
            }
        }

        private void UpdateHex()
        {
            _hex.value = ColorPickerHelper.ColorToHex(Color);
        }
    }
}
