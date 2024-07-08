using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatch : VisualElement
    {
        public const string UssClassName = "rosettaui-colorpicker-swatch";
        public const string EnableTextUssClassName = UssClassName + "--enable-text";
        public const string TileClassName = UssClassName + "__tile";
        public const string TileCurrentClassName = TileClassName + "-current";
        public const string TileCoreClassName = TileClassName + "-core";
        public const string TileColorUssClassName = TileClassName + "-color";
        public const string TileColorOpaqueUssClassName = TileClassName + "-color-opaque";
        public const string TextContainerUssClassName = UssClassName + "__text-container";
        public const string TextInputUssClassName = TextContainerUssClassName + "__text-input";
        
        private readonly VisualElement _tileElement;
        private readonly VisualElement _colorElement;
        private readonly VisualElement _colorOpaque;
        private readonly VisualElement _textContainer;
        private Label _label;

        public bool IsCurrent
        {
            set
            {
                if (value)
                {
                    _tileElement.AddToClassList(TileCurrentClassName);
                }
                else
                {
                    _tileElement.RemoveFromClassList(TileCurrentClassName);
                }
            }
        }
        
        public Color Color
        {
            get => _colorElement.style.backgroundColor.value;
            set
            {
                _colorElement.style.backgroundColor = value;

                var colorWithoutAlpha = value;
                colorWithoutAlpha.a = 1;
                _colorOpaque.style.unityBackgroundImageTintColor = colorWithoutAlpha;
            }
        }

        public string Label
        {
            get => _label?.text;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if ( _label != null) _textContainer.Remove(_label);
                }
                else
                {
                    _label ??= new Label();

                    _label.text = value;
                    _textContainer.Add(_label);
                }
            }
        }

        public bool EnableText
        {
            set => EnableInClassList(EnableTextUssClassName, value);
        }

        public ColorPickerSwatch()
        {
            AddToClassList(UssClassName);
            
            _tileElement = new VisualElement();
            _tileElement.AddToClassList(TileClassName);
            
            var coreElement = new VisualElement();
            coreElement.AddToClassList(TileCoreClassName);
            
            var checkerboardElement = new Checkerboard(CheckerboardTheme.Light);

            _colorElement = new VisualElement();
            _colorElement.AddToClassList(TileColorUssClassName);

            _colorOpaque = new VisualElement();
            _colorOpaque.AddToClassList(TileColorOpaqueUssClassName);

            _colorElement.Add(_colorOpaque);
            checkerboardElement.Add(_colorElement);
            coreElement.Add(checkerboardElement);
            _tileElement.Add(coreElement);
            
            _textContainer = new VisualElement();
            _textContainer.AddToClassList(TextContainerUssClassName);
            
            Add(_tileElement);
            Add(_textContainer);
        }

        public void StartRename(Action onRenameFinished)
        {
            var textField = new TextField
            {
                value = _label?.text
            };
            textField.AddToClassList(TextInputUssClassName);
            
            _textContainer.Add(textField);
            
            // すぐにFocusするとWarningが出るので少し遅らせる @Unity6000.0.2f1
            schedule.Execute(() => textField.Focus()).ExecuteLater(32);

            DisplayLabel(false);
            
            textField.RegisterCallback<BlurEvent>(OnBlur);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            
            return;
            

            void OnBlur(BlurEvent evt)
            {
                StopRename();
                
                Label = textField.value;
                onRenameFinished?.Invoke();
            }

            void OnDetachFromPanel(DetachFromPanelEvent evt)
            {
                StopRename();
            }
            
            void StopRename()
            {
                textField.UnregisterCallback<BlurEvent>(OnBlur);
                UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

                _textContainer.Remove(textField);
                DisplayLabel(true);
            }
            
            void DisplayLabel(bool display)
            {
                if (_label == null) return;
                _label.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}