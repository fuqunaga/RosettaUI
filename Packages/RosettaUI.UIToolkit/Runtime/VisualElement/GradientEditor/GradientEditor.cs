using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientEditor : VisualElement
    {
        public static readonly string USSClassName = "rosettaui-gradient-editor";

        
        #region static interface

        private static ModalWindow _window;
        private static GradientEditor _gradientEditorInstance;

        #endregion
        
        #region static members
        
        private static VisualTreeAsset _visualTreeAsset;
        private static RenderTexture _gradientTexture;
        private static Texture2D _previewCheckerBoardTexture;

        #endregion
        
        public static void Show(Vector2 position, VisualElement target, Gradient initialGradient,
            Action<Gradient> onGradientChanged)
        {
            if (_window == null)
            {
                _window = new ModalWindow();
                _gradientEditorInstance = new GradientEditor();
                _window.Add(_gradientEditorInstance);

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
            _gradientEditorInstance._gradient = initialGradient;
            _gradientEditorInstance.onGradientChanged += onGradientChanged;
            _gradientEditorInstance.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            return;

            void OnDetach(DetachFromPanelEvent _)
            {
                _gradientEditorInstance.onGradientChanged -= onGradientChanged;
                _gradientEditorInstance.UnregisterCallback<DetachFromPanelEvent>(OnDetach);

                target?.Focus();
            }
        }

        public event Action<Gradient> onGradientChanged;

        private Gradient _gradient;
        private GradientKeysEditor _alphaKeysEditor;
        private GradientKeysEditor _colorKeysEditor;
        private GradientKeysSwatch _selectedSwatch;
                
        private EnumField _modeEnum;
        private VisualElement _gradientPreview;
        private VisualElement _alphaCursorContainer;
        private VisualElement _colorCursorContainer;
        private Slider _alphaSlider;
        private VisualElement _colorFieldBox;
        private VisualElement _colorField;
        private Slider _locationSlider;
        private Button _copyButton;
        private Button _pasteButton;
        private Label _infoLabel;
        
        

        private GradientEditor()
        {
            AddToClassList(USSClassName);
            _visualTreeAsset ??= Resources.Load<VisualTreeAsset>("RosettaUI_GradientEditor");
            _visualTreeAsset.CloneTree(this);

            InitUI();
        }

        
        private void InitUI()
        {
            _modeEnum = this.Q<EnumField>("mode-enum");
            
            var previewBackground = this.Q("preview-background");
            _gradientPreview = this.Q("preview");

            _alphaCursorContainer = this.Q("alpha-cursor-container");
            _colorCursorContainer = this.Q("color-cursor-container");

            _colorFieldBox = this.Q("color-field-box");
            _colorField = _colorFieldBox.Q("color-field");
            _alphaSlider = this.Q("alpha-slider") as Slider;
            _locationSlider = this.Q("location-slider") as Slider;

            _modeEnum.RegisterValueChangedCallback(_ => OnGradientChanged());
            
            _colorField.RegisterCallback<PointerDownEvent>(evt =>
            {
                var position = evt.position;
                ColorPicker.Show(position, _colorField, _selectedSwatch.Color, color =>
                {
                    _colorField.style.backgroundColor = color;
                    _selectedSwatch.Color = color;
                    OnGradientChanged();
                });
            });

            _alphaSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                
                _selectedSwatch.Alpha = evt.newValue;
                OnGradientChanged();
            });

            _locationSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                
                _selectedSwatch.Time = Mathf.Clamp01(_locationSlider.value / 100f);
                OnGradientChanged();
            });

            this.ScheduleToUseResolvedLayoutBeforeRendering(() =>
            {
                _modeEnum.value = _gradient.mode;
                InitPreviewBackground(previewBackground);
                UpdateGradientPreview();
                InitAlphaKeysEditor();
                InitColorKeysEditor();
                InitGradientCode();
                
                _selectedSwatch = _colorKeysEditor.ShowedSwatches.FirstOrDefault();
                
                // はみ出し抑制
                VisualElementExtension.CheckOutOfScreen(_window.Position, _window);
            });
        }



        private static void InitPreviewBackground(VisualElement previewBackground)
        {
            if (_previewCheckerBoardTexture == null)
            {
                const int gridSize = 6;
                var rs = previewBackground.resolvedStyle;
                var size = new Vector2Int(Mathf.CeilToInt(rs.width),  Mathf.CeilToInt(rs.height));
                _previewCheckerBoardTexture = TextureUtility.CreateCheckerBoardTexture(size, gridSize);
            }   
            
            previewBackground.style.backgroundImage = _previewCheckerBoardTexture;
        }

        private void InitAlphaKeysEditor()
        {
            var swatches = _gradient.alphaKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Alpha = ak.alpha
            }).ToList();
            
            _alphaKeysEditor = new GradientKeysEditor(_gradient, _alphaCursorContainer, swatches,
                onSwatchChanged: OnSwatchChanged,
                isAlpha: true
                );
        }

        private void InitColorKeysEditor()
        {
            var swatches = _gradient.colorKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Color = ak.color
            }).ToList();
            
            _colorKeysEditor = new GradientKeysEditor(_gradient, _colorCursorContainer, swatches,
                onSwatchChanged: OnSwatchChanged,
                isAlpha: false
            );
        }

        public void ResetUI()
        {
            // if (_alphaSwatches != null)
            // {
            //     foreach (var swatch in _alphaSwatches)
            //     {
            //         swatch.cursor.RemoveFromHierarchy();
            //         swatch.cursor = null;
            //     }
            //
            //     _alphaSwatches.Clear();
            // }

            // if (_colorSwatches != null)
            // {
            //     foreach (var swatch in _colorSwatches)
            //     {
            //         swatch.cursor.RemoveFromHierarchy();
            //         swatch.cursor = null;
            //     }
            //
            //     _colorSwatches.Clear();
            // }

            _selectedSwatch = null;

            // BuildArrays();
            // UpdateSwatches(_alphaSwatches, _alphaCursorContainer);
            // UpdateSwatches(_colorSwatches, _colorCursors);
        }

        private void OnGradientChanged()
        {
            UpdateGradient();
            UpdateGradientPreview();
            onGradientChanged?.Invoke(_gradient);
        }
        
        private void UpdateGradient()
        {
            // GradientColorKey[] colorKeys = new GradientColorKey[_colorSwatches.Count];
            // for (int i = 0; i < colorKeys.Length; i++)
            // {
            //     colorKeys[i] = new GradientColorKey(_colorSwatches[i].color, _colorSwatches[i].time);
            // }

            var alphaSwatches = _alphaKeysEditor.ShowedSwatches;
            var alphaKeys = alphaSwatches.Select(swatch => new GradientAlphaKey(swatch.Alpha, swatch.Time)).ToArray();
            
            _gradient.SetKeys(_gradient.colorKeys, alphaKeys);
            _gradient.mode = _modeEnum.value as GradientMode? ?? GradientMode.Blend;
        }
        
        private void UpdateGradientPreview()
        {
            var gradientTexture = GradientPickerHelper.GenerateGradientPreview(_gradient, _gradientPreview.style.backgroundImage.value.texture);
            _gradientPreview.style.backgroundImage = gradientTexture;
        }

        
        private void OnSwatchChanged(GradientKeysSwatch selectedSwatch)
        {
            UpdateSelectedSwatchField(selectedSwatch);
            OnGradientChanged();
        }

        
        private void UpdateSelectedSwatchField(GradientKeysSwatch swatch)
        {
            if (swatch == null)
            {
                _colorFieldBox.visible = false;
                _alphaSlider.visible = false;
                _locationSlider.visible = false;
                return;
            }
            
            _selectedSwatch = swatch;
            var isAlpha = swatch.IsAlpha;
            
            _alphaSlider.visible = isAlpha;
            _colorFieldBox.visible = !isAlpha;
            
            if (isAlpha)
            {
                _alphaSlider.value = swatch.Alpha;
            }
            else
            {
                _colorField.style.backgroundColor = swatch.Color;
            }

            _locationSlider.visible = true;
            _locationSlider.value = swatch.Time * 100f;
        }


        private void InitGradientCode()
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

        

        #region Color Cursors Events

        private void OnPointerDownOnColorCursors(PointerDownEvent evt)
        {
            // var localPos = _colorCursors.WorldToLocal(evt.position);
            //
            // var swatch = ContainsSwatchAtPosition(localPos, _colorSwatches, false);
            // if (swatch != null)
            // {
            //     // _selectedSwatch = swatch;
            //     SelectSwatch(swatch);
            // }
            // else
            // {
            //     // 無かったら新規追加
            //     if (_colorSwatches.Count < MaxKeyNum)
            //     {
            //         var t = LocalPosToTime(_colorCursors, localPos.x);
            //         var c = _gradient.Evaluate(t);
            //         c.a = 1;
            //         var newSwatch = CreateSwatch(t, c, false);
            //         _colorSwatches.Add(newSwatch);
            //         _colorCursors.Add(newSwatch.cursor);
            //         newSwatch.cursor.style.left = localPos.x;
            //         SelectSwatch(newSwatch);
            //     }
            //     else
            //     {
            //         Debug.LogWarning("Max 8 color keys and 8 alpha keys are allowed in a gradient.");
            //     }
            // }
            //
            // isClicking = true;
            //
            // evt.StopPropagation();
        }

        // private void OnPointerMoveOnColorCursors(PointerMoveEvent evt)
        // {
        //     if (isClicking)
        //     {
        //         var localPos = _colorCursors.WorldToLocal(evt.position);
        //
        //         if (_selectedSwatch != null)
        //         {
        //             var rect = CalcRect(_colorCursors);
        //             rect.y = 0;
        //             if (rect.Contains(localPos))
        //             {
        //                 _selectedSwatch.time = LocalPosToTime(_colorCursors, localPos.x);
        //                 UpdateSelectedSwatchField();
        //                 UpdateSwatches(_colorSwatches, _colorCursors);
        //                 OnGradientChanged();
        //             }
        //         }
        //     }
        //
        //     evt.StopPropagation();
        // }
        //
        // private void OnPointerUpOnColorCursors(PointerUpEvent evt)
        // {
        //     isClicking = false;
        //     evt.StopPropagation();
        // }
        //
        // private void OnPointerLeaveColorCursors(PointerLeaveEvent evt)
        // {
        //     CheckCursorLeave(evt, _colorCursors, _colorSwatches);
        //
        //     evt.StopPropagation();
        // }

        #endregion
    }
}