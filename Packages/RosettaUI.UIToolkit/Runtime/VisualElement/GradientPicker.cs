using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientPicker : VisualElement
    {
        [Serializable]
        public enum Mode
        {
            Blend = 0,
            Fixed = 1,
#if UNITY_2022_3_OR_NEWER
            PerceptualBlend = 2,
#endif
        }

        #region static interface

        private static ModalWindow _window;
        private static GradientPicker _gradientPickerInstance;

        public static int TextDigit { get; set; } = 3;
        public static int MaxKeyNum { get; set; } = 8;

        #endregion

        private Gradient PreviewGradient
        {
            get => _gradient;
            set
            {
                _gradient = value;
                Texture2D gradientTexture =
                    GradientPickerHelper.GenerateGradientPreview(value,
                        _gradientPreview.style.backgroundImage.value.texture);
                _gradientPreview.style.backgroundImage = gradientTexture;
                _gradientMode = value.mode;
            }
        }

        private Gradient _gradient;
        private GradientMode _gradientMode = GradientMode.Blend;
        
        public static void Show(Vector2 position, VisualElement target, Gradient initialGradient,
            Action<Gradient> onGradientChanged)
        {
            if (_window == null)
            {
                _window = new ModalWindow();
                _gradientPickerInstance = new GradientPicker();
                _window.Add(_gradientPickerInstance);

                _window.RegisterCallback<NavigationSubmitEvent>(_ => _window.Hide());
                _window.RegisterCallback<NavigationCancelEvent>(_ =>
                {
                    onGradientChanged?.Invoke(initialGradient);
                    _window.Hide();
                });
            }

            _window.Show(position, target);


            // はみ出し抑制
            if(!float.IsNaN(_window.resolvedStyle.width) && !float.IsNaN(_window.resolvedStyle.height))
            {
                VisualElementExtension.CheckOutOfScreen(position, _window);
            }
            
            // Show()前はPanelが設定されていないのでコールバック系はShow()後
            _gradientPickerInstance.PreviewGradient = initialGradient;
            _gradientPickerInstance.onGradientChanged += onGradientChanged;
            _gradientPickerInstance.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            void OnDetach(DetachFromPanelEvent _)
            {
                _gradientPickerInstance.onGradientChanged -= onGradientChanged;
                _gradientPickerInstance.UnregisterCallback<DetachFromPanelEvent>(OnDetach);

                target?.Focus();
            }

            if (_gradientPickerInstance.isInitialized)
            {
                _gradientPickerInstance.ResetUI();
            }
        }

        #region static members

        private static VisualTreeAsset _visualTreeAsset;
        private static RenderTexture _gradientTexture;

        #endregion

        public event Action<Gradient> onGradientChanged;

        public bool isInitialized = false;

        private DropdownField _modeEnum;

        private VisualElement _gradientPreview;
        private VisualElement _alphaCursors;
        private VisualElement _colorCursors;

        private Slider _alphaSlider;
        private VisualElement _colorFieldBox;
        private VisualElement _colorField;
        private Slider _locationSlider;
        private Button _copyButton;
        private Button _pasteButton;
        private Label _infoLabel;
        
        private bool isClicking = false;

        private class Swatch
        {
            public float Time;
            public Color Color;
            public bool IsAlpha;
            public VisualElement Cursor;

            public Swatch(float time, Color value, bool isAlpha, VisualElement cursor)
            {
                Time = time;
                Color = value;
                IsAlpha = isAlpha;
                Cursor = cursor;
            }
        }

        private List<Swatch> _alphaSwatches = new List<Swatch>();
        private List<Swatch> _colorSwatches = new List<Swatch>();
        private Swatch _selectedSwatch = null;

        static float Round(float value, int digit)
        {
            var scale = Mathf.Pow(10f, digit);
            return Mathf.Round(value * scale) / scale;
        }

        private GradientPicker()
        {
            _visualTreeAsset ??= Resources.Load<VisualTreeAsset>("RosettaUI_GradientPicker");
            _visualTreeAsset.CloneTree(this);

            InitUI();
        }

        void BuildArrays()
        {
            if (_gradient == null)
                return;

            GradientColorKey[] colorKeys = _gradient.colorKeys;
            _colorSwatches.Clear();
            for (int i = 0; i < colorKeys.Length; i++)
            {
                Color color = colorKeys[i].color;
                color.a = 1f;
                var swatch = CreateSwatch(colorKeys[i].time, color, false);
                _colorSwatches.Add(swatch);
                _colorCursors.Add(swatch.Cursor);
            }

            GradientAlphaKey[] alphaKeys = _gradient.alphaKeys;
            _alphaSwatches.Clear();
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                float a = alphaKeys[i].alpha;
                var swatch = CreateSwatch(alphaKeys[i].time, new Color(a, a, a, 1), true);
                _alphaSwatches.Add(swatch);
                _alphaCursors.Add(swatch.Cursor);
            }

            _gradientMode = _gradient.mode;
            _modeEnum.index = (int) _gradientMode;
            
            SelectSwatch(_colorSwatches[0]);
        }

        public void ResetUI()
        {
            if (_alphaSwatches != null)
            {
                foreach (var swatch in _alphaSwatches)
                {
                    swatch.Cursor.RemoveFromHierarchy();
                    swatch.Cursor = null;
                }

                _alphaSwatches.Clear();
            }

            if (_colorSwatches != null)
            {
                foreach (var swatch in _colorSwatches)
                {
                    swatch.Cursor.RemoveFromHierarchy();
                    swatch.Cursor = null;
                }

                _colorSwatches.Clear();
            }

            _selectedSwatch = null;

            BuildArrays();
            UpdateSwatches(_alphaSwatches, _alphaCursors);
            UpdateSwatches(_colorSwatches, _colorCursors);
        }

        void UpdateGradient()
        {
            GradientColorKey[] colorKeys = new GradientColorKey[_colorSwatches.Count];
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(_colorSwatches[i].Color, _colorSwatches[i].Time);
            }

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[_alphaSwatches.Count];
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i] = new GradientAlphaKey(_alphaSwatches[i].Color.r, _alphaSwatches[i].Time);
            }

            var newGradient = new Gradient();
            newGradient.SetKeys(colorKeys, alphaKeys);
            newGradient.mode = _gradientMode;
            PreviewGradient = newGradient;
        }

        void OnGradientChanged()
        {
            UpdateGradient();
            onGradientChanged?.Invoke(PreviewGradient);
        }

        Swatch CreateSwatch(float time, Color color, bool isAlpha)
        {
            var cursor = CreateCursor(isAlpha);
            cursor.style.unityBackgroundImageTintColor = color;
            var swatch = new Swatch(time, color, isAlpha, cursor);
            return swatch;
        }

        VisualElement CreateCursor(bool isAlpha = false)
        {
            var cursor = new VisualElement();
            if (isAlpha)
            {
                cursor.AddToClassList("rosettaui-gradientpicker__cursor");
            }
            else
            {
                cursor.AddToClassList("rosettaui-gradientpicker__cursor__color");
            }

            cursor.focusable = true;
            return cursor;
        }

        float TimeToLocalPos(VisualElement element, float t)
        {
            var f = element.resolvedStyle.width * t;
            return f;
        }

        float LocalPosToTime(VisualElement element, float x)
        {
            var f = x / element.resolvedStyle.width;
            return f;
        }

        void UpdateSelectedSwatchField()
        {
            if (_selectedSwatch == null)
                return;
            if (_selectedSwatch.IsAlpha)
            {
                _alphaSlider.visible = true;
                _alphaSlider.value = _selectedSwatch.Color.r;

                _colorFieldBox.visible = false;
            }
            else
            {
                _colorFieldBox.visible = true;
                _colorField.style.backgroundColor = _selectedSwatch.Color;

                _alphaSlider.visible = false;
            }

            _locationSlider.visible = true;
            _locationSlider.value = _selectedSwatch.Time * 100f;
        }

        void UpdateSwatches(List<Swatch> swatches, VisualElement element)
        {
            swatches.Sort((a, b) => a.Time.CompareTo(b.Time));

            foreach (var swatch in swatches)
            {
                var c = swatch.Cursor.style;
                c.left = TimeToLocalPos(element, swatch.Time);
                c.unityBackgroundImageTintColor = swatch.Color;
            }
        }

        void RemoveSwatch(Swatch swatch, List<Swatch> swatches, VisualElement element)
        {
            if (swatches.Contains(swatch))
            {
                swatches.Remove(swatch);
            }

            swatch.Cursor.RemoveFromHierarchy();
            UpdateSwatches(swatches, element);
        }

        void SelectSwatch(Swatch swatch)
        {
            UnselectSwatch();

            _selectedSwatch = swatch;

            UpdateSelectedSwatchField();

            _selectedSwatch.Cursor.Focus();
        }

        void UnselectSwatch()
        {
            if (_selectedSwatch == null)
                return;

            _selectedSwatch = null;
            _alphaSlider.visible = false;
            _colorFieldBox.visible = false;
            _locationSlider.visible = false;
        }

        private void InitUI()
        {
            _gradientPreview = this.Q("gradient-preview");

            _alphaCursors = this.Q("alpha-cursors");
            _colorCursors = this.Q("color-cursors");

            _colorFieldBox = this.Q("color-field-box");
            _colorField = _colorFieldBox.Q("color-field");

            _modeEnum = this.Q("mode-enum") as DropdownField;
            _alphaSlider = this.Q("alpha-slider") as Slider;
            _locationSlider = this.Q("location-slider") as Slider;

            _alphaCursors.RegisterCallback<PointerDownEvent>(OnPointerDownOnAlphaCursors);
            _alphaCursors.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnAlphaCursors);
            _alphaCursors.RegisterCallback<PointerUpEvent>(OnPointerUpOnAlphaCursors);
            _alphaCursors.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveAlphaCursors);

            _colorCursors.RegisterCallback<PointerDownEvent>(OnPointerDownOnColorCursors);
            _colorCursors.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnColorCursors);
            _colorCursors.RegisterCallback<PointerUpEvent>(OnPointerUpOnColorCursors);
            _colorCursors.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveColorCursors);

            _modeEnum.RegisterValueChangedCallback(evt => { SelectMode(); });

#if UNITY_2022_3_OR_NEWER
            _modeEnum.choices.Add("PerceptualBlend");
#endif
            
            _colorField.RegisterCallback<PointerDownEvent>(evt =>
            {
                var position = evt.position;
                ColorPicker.Show(position, _colorField, _selectedSwatch.Color, color =>
                {
                    _colorField.style.backgroundColor = color;
                    _selectedSwatch.Color = color;
                    UpdateSwatches(_colorSwatches, _colorCursors);
                    OnGradientChanged();
                });
            });

            _alphaSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;

                var a = Round(evt.newValue, TextDigit);
                _selectedSwatch.Color = new Color(a, a, a, 1);
                UpdateSwatches(_alphaSwatches, _alphaCursors);
                OnGradientChanged();
            });

            _locationSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;

                var t = Round(Mathf.Clamp01(_locationSlider.value / 100f), TextDigit);
                _selectedSwatch.Time = t;
                if (_selectedSwatch.IsAlpha)
                {
                    UpdateSwatches(_alphaSwatches, _alphaCursors);
                }
                else
                {
                    UpdateSwatches(_colorSwatches, _colorCursors);
                }

                OnGradientChanged();
            });

            this.ScheduleToUseResolvedLayoutBeforeRendering(() =>
            {
                BuildArrays();
                InitGradientCode();
                UpdateSwatches(_alphaSwatches, _alphaCursors);
                UpdateSwatches(_colorSwatches, _colorCursors);
                isInitialized = true;
                
                // はみ出し抑制
                VisualElementExtension.CheckOutOfScreen(_window.Position, _window);
            });
        }

        void InitGradientCode()
        {
            _copyButton = this.Q<Button>("copy-button");
            _pasteButton = this.Q<Button>("paste-button");
            _infoLabel = this.Q<Label>("info-label");
            _infoLabel.text = "";
            
            _copyButton.clicked += () =>
            {
                var json = GradientPickerHelper.GradientToJson(_gradient);
                if (json != null)
                {
                    GUIUtility.systemCopyBuffer = json;
                    _infoLabel.text = "Copy!";
                }
            };

            _pasteButton.clicked += () =>
            {
                var clip = GUIUtility.systemCopyBuffer;
                if (clip != null)
                {
                    if (GradientPickerHelper.JsonToGradient(clip) is { } gradient)
                    {
                        _gradient = gradient;
                        ResetUI();
                        OnGradientChanged();
                        _infoLabel.text = "Paste!";
                    }
                }
            };
        }
        
        Rect CalcRect(VisualElement cursor)
        {
            Rect rect = new Rect();
            var cursorStyle = cursor.resolvedStyle;
            rect.x = cursorStyle.left;
            rect.y = cursorStyle.top;
            rect.width = cursorStyle.width;
            rect.height = cursorStyle.height;
            return rect;
        }

        Swatch ContainsSwatchAtPosition(Vector2 position, List<Swatch> swatches, bool isAlpha)
        {
            Swatch swatch = null;

            if (_selectedSwatch != null && swatches.Contains(_selectedSwatch) && _selectedSwatch.IsAlpha == isAlpha)
            {
                var rect = CalcRect(_selectedSwatch.Cursor);
                if (rect.Contains(position))
                {
                    return _selectedSwatch;
                }
            }

            for (int i = 0; i < swatches.Count; i++)
            {
                var rect = CalcRect(swatches[i].Cursor);

                if (rect.Contains(position) && swatches[i].IsAlpha == isAlpha)
                {
                    swatch = swatches[i];
                    break;
                }
            }

            return swatch;
        }

        void CheckCursorLeave(PointerLeaveEvent evt, VisualElement cursors, List<Swatch> swatches)
        {
            var localPos = cursors.WorldToLocal(evt.position);

            if (isClicking && _selectedSwatch != null)
            {
                if (localPos.y < 0 || localPos.y >= (cursors.resolvedStyle.height))
                {
                    // 縦にはみ出したら削除
                    RemoveSwatch(_selectedSwatch, swatches, cursors);
                    UnselectSwatch();
                    OnGradientChanged();
                }

                isClicking = false;
            }
        }

        #region Alpha Cursors Events

        void OnPointerDownOnAlphaCursors(PointerDownEvent evt)
        {
            var localPos = _alphaCursors.WorldToLocal(evt.position);

            var swatch = ContainsSwatchAtPosition(localPos, _alphaSwatches, true);
            if (swatch != null)
            {
                _selectedSwatch = swatch;
                SelectSwatch(swatch);
            }
            else
            {
                // 無かったら新規追加
                if (_alphaSwatches.Count < MaxKeyNum)
                {
                    var t = LocalPosToTime(_alphaCursors, localPos.x);
                    var a = _gradient.Evaluate(t).a;
                    var newSwatch = CreateSwatch(t, new Color(a, a, a, 1), true);
                    _alphaSwatches.Add(newSwatch);
                    _alphaCursors.Add(newSwatch.Cursor);
                    newSwatch.Cursor.style.left = localPos.x;
                    SelectSwatch(newSwatch);
                }
                else
                {
                    Debug.LogWarning("Max 8 color keys and 8 alpha keys are allowed in a gradient.");
                }
            }

            isClicking = true;

            evt.StopPropagation();
        }

        void OnPointerMoveOnAlphaCursors(PointerMoveEvent evt)
        {
            if (isClicking)
            {
                var localPos = _alphaCursors.WorldToLocal(evt.position);

                if (_selectedSwatch != null)
                {
                    var rect = CalcRect(_alphaCursors);
                    rect.y = 0;
                    if (rect.Contains(localPos))
                    {
                        _selectedSwatch.Time = LocalPosToTime(_alphaCursors, localPos.x);
                        UpdateSelectedSwatchField();
                        UpdateSwatches(_alphaSwatches, _alphaCursors);
                        OnGradientChanged();
                    }
                }
            }

            evt.StopPropagation();
        }

        void OnPointerUpOnAlphaCursors(PointerUpEvent evt)
        {
            isClicking = false;
            evt.StopPropagation();
        }

        void OnPointerLeaveAlphaCursors(PointerLeaveEvent evt)
        {
            CheckCursorLeave(evt, _alphaCursors, _alphaSwatches);

            evt.StopPropagation();
        }

        #endregion

        #region Color Cursors Events

        void OnPointerDownOnColorCursors(PointerDownEvent evt)
        {
            var localPos = _colorCursors.WorldToLocal(evt.position);

            var swatch = ContainsSwatchAtPosition(localPos, _colorSwatches, false);
            if (swatch != null)
            {
                // _selectedSwatch = swatch;
                SelectSwatch(swatch);
            }
            else
            {
                // 無かったら新規追加
                if (_colorSwatches.Count < MaxKeyNum)
                {
                    var t = LocalPosToTime(_colorCursors, localPos.x);
                    var c = _gradient.Evaluate(t);
                    c.a = 1;
                    var newSwatch = CreateSwatch(t, c, false);
                    _colorSwatches.Add(newSwatch);
                    _colorCursors.Add(newSwatch.Cursor);
                    newSwatch.Cursor.style.left = localPos.x;
                    SelectSwatch(newSwatch);
                }
                else
                {
                    Debug.LogWarning("Max 8 color keys and 8 alpha keys are allowed in a gradient.");
                }
            }

            isClicking = true;

            evt.StopPropagation();
        }

        void OnPointerMoveOnColorCursors(PointerMoveEvent evt)
        {
            if (isClicking)
            {
                var localPos = _colorCursors.WorldToLocal(evt.position);

                if (_selectedSwatch != null)
                {
                    var rect = CalcRect(_colorCursors);
                    rect.y = 0;
                    if (rect.Contains(localPos))
                    {
                        _selectedSwatch.Time = LocalPosToTime(_colorCursors, localPos.x);
                        UpdateSelectedSwatchField();
                        UpdateSwatches(_colorSwatches, _colorCursors);
                        OnGradientChanged();
                    }
                }
            }

            evt.StopPropagation();
        }

        void OnPointerUpOnColorCursors(PointerUpEvent evt)
        {
            isClicking = false;
            evt.StopPropagation();
        }

        void OnPointerLeaveColorCursors(PointerLeaveEvent evt)
        {
            CheckCursorLeave(evt, _colorCursors, _colorSwatches);

            evt.StopPropagation();
        }

        #endregion

        void SelectMode()
        {
            _gradientMode = (GradientMode) _modeEnum.index;
            OnGradientChanged();
        }
    }
}