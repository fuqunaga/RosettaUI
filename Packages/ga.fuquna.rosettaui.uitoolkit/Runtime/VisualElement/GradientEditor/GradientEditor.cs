using System;
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
                _window = new ModalWindow(true);
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
            _gradientEditorInstance.Initialize(initialGradient);
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
        private VisualElement _propertyGroup;
        private Slider _alphaSlider;
        private ColorField _colorField;
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
            
            _gradientPreview = new VisualElement()
            {
                style =
                {
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                }
            };
            
            var checkerboard = new Checkerboard(CheckerboardTheme.Dark);
            
            var previewBackground = this.Q("preview-background");
            
            checkerboard.Add(_gradientPreview);
            previewBackground.Add(checkerboard);

            _alphaCursorContainer = this.Q("alpha-cursor-container");
            _colorCursorContainer = this.Q("color-cursor-container");

            _propertyGroup = this.Q("property-group");
            
            _colorField = new ColorField("Color")
            {
                EnableAlpha = false
            };
            var valueFieldContainer = this.Q("value-field-container");
            valueFieldContainer.Add(_colorField);
            
            _alphaSlider = this.Q("alpha-slider") as Slider;
            _locationSlider = this.Q("location-slider") as Slider;

            _modeEnum.RegisterValueChangedCallback(_ => OnGradientChanged());
            
            _colorField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                
                _selectedSwatch.Color = evt.newValue;
                OnGradientChanged();
            });
            
            _alphaSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                
                _selectedSwatch.Alpha = evt.newValue;
                OnGradientChanged();
            });

            _locationSlider.RegisterValueChangedCallback(_ =>
            {
                if (_selectedSwatch == null) return;

                _selectedSwatch.TimePercent = _locationSlider.value;
                OnGradientChanged();
            });

            InitGradientCode();
            
            this.ScheduleToUseResolvedLayoutBeforeRendering(() =>
            {
                // はみ出し抑制
                VisualElementExtension.CheckOutOfScreen(_window.Position, _window);
            });
        }

        public void Initialize(Gradient gradient)
        {
            _gradient = gradient;
            _modeEnum.value = _gradient.mode;
            UpdateGradientPreview();
            InitAlphaKeysEditor();
            InitColorKeysEditor();
            
            UpdateSelectedSwatchField(_colorKeysEditor.ShowedSwatches.FirstOrDefault());
        }

        private void InitAlphaKeysEditor()
        {
            _alphaKeysEditor ??= new GradientKeysEditor(
                _alphaCursorContainer,
                onSwatchChanged: OnSwatchChanged,
                isAlpha: true
            );

            var swatches = _gradient.alphaKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Alpha = ak.alpha
            });

            _alphaKeysEditor.Initialize(_gradient, swatches);
        }

        private void InitColorKeysEditor()
        {
            _colorKeysEditor ??= new GradientKeysEditor(
                _colorCursorContainer, 
                onSwatchChanged: OnSwatchChanged,
                isAlpha: false
            );

            var swatches = _gradient.colorKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Color = ak.color
            });

            _colorKeysEditor.Initialize(_gradient, swatches);
        }

        private void OnGradientChanged()
        {
            UpdateGradient();
            UpdateGradientPreview();
            onGradientChanged?.Invoke(_gradient);
        }
        
        private void UpdateGradient()
        {
            var colorSwatches = _colorKeysEditor.ShowedSwatches;
            var colorKeys = colorSwatches.Select(swatch => new GradientColorKey(swatch.Color, swatch.Time)).ToArray();

            var alphaSwatches = _alphaKeysEditor.ShowedSwatches;
            var alphaKeys = alphaSwatches.Select(swatch => new GradientAlphaKey(swatch.Alpha, swatch.Time)).ToArray();
            
            _gradient.SetKeys(colorKeys, alphaKeys);
            _gradient.mode = _modeEnum.value as GradientMode? ?? GradientMode.Blend;
        }
        
        private void UpdateGradientPreview()
        {
            var gradientTexture = GradientHelper.GenerateGradientPreview(_gradient, _gradientPreview.style.backgroundImage.value.texture);
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
                _propertyGroup.visible = false;
                _locationSlider.visible = false;
                return;
            }
            
            _propertyGroup.visible = true;
            
            _selectedSwatch = swatch;
            var isAlpha = swatch.IsAlpha;
            
            SetDisplay(_alphaSlider, isAlpha);
            SetDisplay(_colorField, !isAlpha);
            
            if (isAlpha)
            {
                _alphaSlider.value = swatch.Alpha;
            }
            else
            {
                _colorField.value = swatch.Color;
            }

            _locationSlider.visible = true;
            _locationSlider.value = swatch.TimePercent;

            return;
            
            void SetDisplay(VisualElement element, bool display)
            {
                element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }


        private void InitGradientCode()
        {
            _copyButton = this.Q<Button>("copy-button");
            _pasteButton = this.Q<Button>("paste-button");
            _infoLabel = this.Q<Label>("info-label");
            _infoLabel.text = "";
            
            _copyButton.clicked += () =>
            {
                var json = GradientHelper.GradientToJson(_gradient);
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
                    if (GradientHelper.JsonToGradient(clip) is { } gradient)
                    {
                        Initialize(gradient);
                        UpdateGradientPreview();
                        onGradientChanged?.Invoke(_gradient);
                        _infoLabel.text = "Paste!";
                    }
                }
            };
        }
    }
}