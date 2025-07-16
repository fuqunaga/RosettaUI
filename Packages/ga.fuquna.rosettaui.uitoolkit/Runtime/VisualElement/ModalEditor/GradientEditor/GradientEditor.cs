using System;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientEditor : ModalEditor<Gradient>
    {
        public const string USSClassName = "rosettaui-gradient-editor";
        
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public static string visualTreeAssetName = "RosettaUI_GradientEditor";

        #region Static Window Management 
        
        private static GradientEditor _instance;
        private static VisualTreeAsset _visualTreeAsset;
        
        static GradientEditor()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                _instance = null;
                _visualTreeAsset = null;
            });
        }

        public static void Show(Vector2 position, VisualElement target, Gradient initialGradient,
            Action<Gradient> onGradientChanged)
        {
            _instance ??= new GradientEditor();
            _instance.Show(position, target, onGradientChanged);
            
            
            _instance.SetGradient(initialGradient);
        }
        
        #endregion

        
        private Gradient _gradient;
        private GradientKeysEditor _alphaKeysEditor;
        private GradientKeysEditor _colorKeysEditor;
        private GradientKeysSwatch _selectedSwatch;
        private GradientEditorPresetSet _presetSet;
                
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
        

        // ReSharper disable once MemberCanBePrivate.Global
        public GradientEditor()
        {
            _visualTreeAsset ??= Resources.Load<VisualTreeAsset>(visualTreeAssetName);
            _visualTreeAsset.CloneTree(this);
            
            AddToClassList(USSClassName);

            InitUI();
        }

        
        private void InitUI()
        {
            _modeEnum = this.Q<EnumField>("mode-enum");

            _gradientPreview = this.Q("preview-front");
            
            var previewBackground = this.Q("preview-background");
            Checkerboard.SetupAsCheckerboard(previewBackground, CheckerboardTheme.Dark);
            previewBackground.Add(_gradientPreview);

            _alphaCursorContainer = this.Q("alpha-cursor-container");
            _colorCursorContainer = this.Q("color-cursor-container");

            _propertyGroup = this.Q("property-group");
            
            _colorField = new ColorField("Color")
            {
                EnableAlpha = false
            };
            _colorField.AddManipulator(new PopupMenuManipulator(
                ClipboardUtility.GenerateContextMenuItemsFunc(() => _colorField.value, v => _colorField.value = v))
            );
            var valueFieldContainer = this.Q("value-field-container");
            valueFieldContainer.Add(_colorField);
            
            _alphaSlider = this.Q("alpha-slider") as Slider;
            _locationSlider = this.Q("location-slider") as Slider;

            _modeEnum.RegisterValueChangedCallback(_ => UpdateGradient());
            
            _colorField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                
                _selectedSwatch.Color = evt.newValue;
                UpdateGradient();
            });
            
            _alphaSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                
                _selectedSwatch.Alpha = evt.newValue;
                UpdateGradient();
            });

            _locationSlider.RegisterValueChangedCallback(_ =>
            {
                if (_selectedSwatch == null) return;

                _selectedSwatch.TimePercent = _locationSlider.value;
                UpdateGradient();
            });

            _presetSet = new GradientEditorPresetSet(gradient =>
            {
                SetGradient(gradient);
                NotifyEditorValueChanged(_gradient);
            });
            Add(_presetSet);
        }

        // GradientEditor外部から値をセットする
        // - GradientEditorを開いたとき
        // - Presetが選択されたとき
        public void SetGradient(Gradient gradient)
        {
            _gradient = gradient;
            _modeEnum.SetValueWithoutNotify(_gradient.mode);
            UpdateGradientPreview();
            UpdateAlphaKeysEditor();
            UpdateColorKeysEditor();
            UpdateSelectedSwatchField(_colorKeysEditor.ShowedSwatches.FirstOrDefault());
            
            _presetSet.SetValue(_gradient);
        }

        private void UpdateAlphaKeysEditor()
        {
            _alphaKeysEditor ??= new GradientKeysEditor(
                _alphaCursorContainer,
                OnAddSwatch,
                OnRemoveSwatch,
                OnSelectedSwatchChanged,
                OnSwatchValueChanged,
                isAlpha: true
            );

            var swatches = _gradient.alphaKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Alpha = ak.alpha
            });

            _alphaKeysEditor.Initialize(_gradient, swatches);
        }

        private void UpdateColorKeysEditor()
        {
            _colorKeysEditor ??= new GradientKeysEditor(
                _colorCursorContainer,
                OnAddSwatch,
                OnRemoveSwatch,
                OnSelectedSwatchChanged,
                OnSwatchValueChanged,
                isAlpha: false
            );

            var swatches = _gradient.colorKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Color = ak.color
            });

            _colorKeysEditor.Initialize(_gradient, swatches);
        }

        // GradientEditorのUIから値が変更されたとき
        private void OnGradientChanged()
        {
            UpdateGradientPreview();
            _presetSet.SetValue(_gradient);
            NotifyEditorValueChanged(_gradient);
        }
        
        private void UpdateGradient()
        {
            var colorSwatches = _colorKeysEditor.ShowedSwatches;
            var colorKeys = colorSwatches.Select(swatch => new GradientColorKey(swatch.Color, swatch.Time)).ToArray();

            var alphaSwatches = _alphaKeysEditor.ShowedSwatches;
            var alphaKeys = alphaSwatches.Select(swatch => new GradientAlphaKey(swatch.Alpha, swatch.Time)).ToArray();
            
            _gradient.SetKeys(colorKeys, alphaKeys);
            _gradient.mode = _modeEnum.value as GradientMode? ?? GradientMode.Blend;

            OnGradientChanged();
        }
        
        private void UpdateGradientPreview()
        {
            GradientVisualElementHelper.UpdateGradientPreviewToBackgroundImage(_gradient, _gradientPreview);
        }

        
        private void OnAddSwatch(GradientKeysSwatch swatch)
        {
            UpdateGradient();
        }
        
        private void OnRemoveSwatch(GradientKeysSwatch swatch)
        {
            UpdateGradient();
        }
        
        private void OnSelectedSwatchChanged(GradientKeysSwatch selectedSwatch)
        {
            UpdateSelectedSwatchField(selectedSwatch);
        }
        private void OnSwatchValueChanged(GradientKeysSwatch selectedSwatch)
        {
            UpdateSelectedSwatchField(selectedSwatch);
            UpdateGradient();
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
                _alphaSlider.SetValueWithoutNotify(swatch.Alpha);
            }
            else
            {
                _colorField.SetValueWithoutNotify(swatch.Color);
            }

            _locationSlider.visible = true;
            _locationSlider.SetValueWithoutNotify(swatch.TimePercent);

            return;
            
            void SetDisplay(VisualElement element, bool display)
            {
                element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}