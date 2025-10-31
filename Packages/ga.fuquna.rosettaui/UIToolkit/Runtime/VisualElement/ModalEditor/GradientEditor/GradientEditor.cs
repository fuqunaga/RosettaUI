using System;
using System.Linq;
using RosettaUI.Builder;
using RosettaUI.UIToolkit.UndoSystem;
using RosettaUI.UndoSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientEditor : ModalEditor<Gradient>
    {
        #region Static Window Management 
        
        private static GradientEditor _instance;
        
        static GradientEditor()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                _instance = null;
            });
        }

        public static void Show(Vector2 position, VisualElement target, Gradient initialGradient,
            Action<Gradient> onGradientChanged, Action<bool> onHide)
        {
            _instance ??= new GradientEditor();
            _instance.Show(position, target, onGradientChanged, onHide);
            
            
            _instance.CopiedValue = initialGradient;
        }
        
        #endregion
        
        
        public const string USSClassName = "rosettaui-gradient-editor";
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static string VisualTreeAssetName { get; set; } = "RosettaUI_GradientEditor";
        
        
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
        
        
        #region ModalEditor

        protected override Gradient CopiedValue
        {
            get => GradientHelper.Clone(_gradient);
            set => SetGradient(GradientHelper.Clone(value));
        }

        #endregion

        
        // ReSharper disable once MemberCanBePrivate.Global
        public GradientEditor() : base(VisualTreeAssetName)
        {
            AddToClassList(USSClassName);

            InitUI();
        }

        public Color Evaluate(float time)
        {
            return _gradient.Evaluate(time);
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

            _modeEnum.RegisterValueChangedCallback(evt =>
            {
                UpdateGradient();
                RecordUndoChangeEvent(_modeEnum, evt);
            });
            
            _colorField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                _selectedSwatch.Color = evt.newValue;
                UpdateGradient();
                
                // Undoの登録はColorField内で行われるためここでは行わない
            });
            
            _alphaSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                _selectedSwatch.Alpha = evt.newValue;
                UpdateGradient();

                RecordUndoChangeEvent(_alphaSlider, evt);
            });

            _locationSlider.RegisterValueChangedCallback(evt =>
            {
                if (_selectedSwatch == null) return;
                _selectedSwatch.TimePercent = evt.newValue;
                UpdateGradient();
                
                RecordUndoChangeEvent(_locationSlider, evt);
            });

            _presetSet = new GradientEditorPresetSet(ApplyPreset);
            
            Add(_presetSet);
        }

        // GradientEditor外部から値をセットする
        // - GradientEditorを開いたとき
        // - Presetが選択されたとき
        private void SetGradient(Gradient gradient)
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
            _alphaKeysEditor ??= new GradientKeysEditor(this, _alphaCursorContainer, isAlpha: true);

            var swatches = _gradient.alphaKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Alpha = ak.alpha
            });

            _alphaKeysEditor.Initialize(swatches);
        }

        private void UpdateColorKeysEditor()
        {
            _colorKeysEditor ??= new GradientKeysEditor(this, _colorCursorContainer, isAlpha: false);

            var swatches = _gradient.colorKeys.Select(ak => new GradientKeysSwatch()
            {
                Time = ak.time,
                Color = ak.color
            });

            _colorKeysEditor.Initialize(swatches);
        }
        
        private void ApplyPreset(Gradient gradient)
        {
            var before = RentAllKeysSnapshot();
            
            CopiedValue = gradient;
            NotifyEditorValueChanged();

            RecordUndoAllKeysSnapshot(before);
        }

        // GradientEditorのUIから値が変更されたとき
        private void OnGradientChanged()
        {
            UpdateGradientPreview();
            _presetSet.SetValue(_gradient);
            NotifyEditorValueChanged();
        }

        public void OnGradientKeysChanged()
        { ;
            if (_selectedSwatch != null)
            {
                var keysEditor = _selectedSwatch.IsAlpha ? _alphaKeysEditor : _colorKeysEditor;
                var isSelectedSwatchExist = keysEditor.ShowedSwatches.Any(s => s.UniqueId == _selectedSwatch.UniqueId);
                UpdateSelectedSwatchField(isSelectedSwatchExist ? _selectedSwatch : null);
            }

            UpdateGradient();
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
            GradientVisualElementHelper.UpdatePreviewToBackgroundImage(_gradient, _gradientPreview);
        }

        public void UpdateSelectedSwatchField(GradientKeysSwatch swatch)
        {
            if (swatch == null)
            {
                _propertyGroup.visible = false;
                _locationSlider.visible = false;
                _selectedSwatch = null;
                return;
            }
            
            _propertyGroup.visible = true;

            if (_selectedSwatch != swatch)
            {
                _selectedSwatch?.Blur();
                _selectedSwatch = swatch;
                _selectedSwatch.Focus();
            }

            var isAlpha = swatch.IsAlpha;
            
            _alphaSlider.SetShow(isAlpha);
            _colorField.SetShow(!isAlpha);
            
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
        }
        
        
        #region Undo
        
        public int GetSelectedSwatchId()
        {
            return _selectedSwatch?.UniqueId ?? -1;
        }
        
        public void SelectSwatchById(int swatchId)
        {
            var swatch = (_colorKeysEditor.ShowedSwatches
                          .Concat(_alphaKeysEditor.ShowedSwatches))
                         .FirstOrDefault(s => s.UniqueId == swatchId);
            
            UpdateSelectedSwatchField(swatch);
        }
        
        private void RecordUndoChangeEvent<TValue>(BaseField<TValue> baseField, ChangeEvent<TValue> evt)
        {
            var selectedSwatchId = _selectedSwatch?.UniqueId ?? -1;
            
            Undo.RecordValueChange(nameof(GradientEditor), evt.previousValue, evt.newValue, value =>
            {
                var swatch = (_colorKeysEditor.ShowedSwatches
                              .Concat(_alphaKeysEditor.ShowedSwatches))
                             .FirstOrDefault(s => s.UniqueId == selectedSwatchId);
                
                if (swatch != null)
                {
                    UpdateSelectedSwatchField(swatch);
                    baseField.value = value;
                }
            });
        }
        
        private (GradientKeysEditor.Snapshot color, GradientKeysEditor.Snapshot alpha) RentAllKeysSnapshot()
        {
            var colorSnapshot = GradientKeysEditor.Snapshot.GetPooled();
            var alphaSnapshot = GradientKeysEditor.Snapshot.GetPooled();
            _colorKeysEditor.TakeSnapshot(colorSnapshot);
            _alphaKeysEditor.TakeSnapshot(alphaSnapshot);
            return (colorSnapshot, alphaSnapshot);
        }
        
        
        /// <summary>
        /// ColorKeysとAlphaKeysの両方のUndoを記録する
        /// </summary>
        private void RecordUndoAllKeysSnapshot((GradientKeysEditor.Snapshot color, GradientKeysEditor.Snapshot alpha) before)
        {
            var after = RentAllKeysSnapshot();
            
            var record = Undo.RecordValueChange($"{nameof(GradientEditor)} Apply Preset", before, after, value =>
            {
                _colorKeysEditor.RestoreSnapshotWithoutNotify(value.color);
                _alphaKeysEditor.RestoreSnapshotWithoutNotify(value.alpha);
                OnGradientKeysChanged();
            });
            
            record.onDispose += () =>
            {
                before.color.Dispose();
                before.alpha.Dispose();
                after.color.Dispose();
                after.alpha.Dispose();
            };
        }
        
        #endregion
    }
}