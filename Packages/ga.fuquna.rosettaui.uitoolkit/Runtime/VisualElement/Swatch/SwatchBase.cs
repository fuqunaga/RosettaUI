using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public abstract class SwatchBase<TValue> : VisualElement
    {
        public const string UssClassName = "rosettaui-swatch";
        public const string EnableTextUssClassName = UssClassName + "--enable-text";
        public const string CurrentClassName = UssClassName + "--current";
        public const string TileClassName = UssClassName + "__tile";
        public const string TileOverlapTextClassName = TileClassName + "__overlap-text";
        public const string TextContainerUssClassName = UssClassName + "__text-container";
        public const string TextInputUssClassName = TextContainerUssClassName + "__text-input";
        
        private readonly VisualElement _textContainer;
        private VisualElement _tileElement;
        private Label _label;
        
        public abstract TValue Value { get; set; }

        public bool IsCurrent
        {
            set => EnableInClassList(CurrentClassName, value);
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

        
        protected SwatchBase()
        {
            AddToClassList(UssClassName);

            _textContainer = new VisualElement();
            _textContainer.AddToClassList(TextContainerUssClassName);
            
            Add(_textContainer);
        }

        protected void SetTileElement(VisualElement tileElement, string overlapText = null)
        {
            if (_tileElement != null)
            {
                Remove(_tileElement);
            }
            
            _tileElement = tileElement;
            _tileElement.AddToClassList(TileClassName);

            if (!string.IsNullOrEmpty(overlapText))
            {
                var overlapLabel = new TextElement { text = overlapText };
                overlapLabel.AddToClassList(TileOverlapTextClassName);
                tileElement.Add(overlapLabel);
            }

            Insert(0, tileElement);
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