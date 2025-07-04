using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientField : BaseField<Gradient>
    {
        public new static readonly string ussClassName = "rosettaui-gradient-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        private static readonly GradientColorKey k_WhiteKeyBegin = new(Color.white, 0);
        private static readonly GradientColorKey k_WhiteKeyEnd = new(Color.white, 1);
        private static readonly GradientAlphaKey k_AlphaKeyBegin = new(1, 0);
        private static readonly GradientAlphaKey k_AlphaKeyEnd = new(1, 1);

        public event Action<Vector2, GradientField> showGradientPickerFunc;

        private bool _valueNull;
        private readonly Background _mDefaultBackground = new();
        private readonly GradientInput _gradientInput;

        public override Gradient value
        {
            get => _valueNull ? null : GradientHelper.Clone(rawValue);
            set
            {
                if (value != null || !_valueNull) // let's not reinitialize an initialized gradient
                {
                    using var evt = ChangeEvent<Gradient>.GetPooled(rawValue, value);
                    evt.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(evt);
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GradientField() : this(null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GradientField(string label) : base(label, new GradientInput())
        {
            _gradientInput = this.Q<GradientInput>();
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);

            _gradientInput.AddToClassList(inputUssClassName);
            _gradientInput.RegisterCallback<ClickEvent>(OnClickInput);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
        }

        private void ShowGradientPicker(Vector2 position)
        {
            showGradientPickerFunc?.Invoke(position, this);
        }

        private void UpdateGradientTexture()
        {
            var preview = _gradientInput.preview;
            
            if (_valueNull || showMixedValue)
            {
                preview.style.backgroundImage = _mDefaultBackground;
            }
            else
            {
                GradientVisualElementHelper.UpdateGradientPreviewToBackgroundImage(rawValue, preview);
            }
        }

        private void OnClickInput(ClickEvent evt)
        {
            ShowGradientPicker(evt.position);

            evt.StopPropagation();
        }

        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            var mousePosition = Input.mousePosition;
            var position = new Vector2(
                mousePosition.x,
                Screen.height - mousePosition.y
            );

            var screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            if (!screenRect.Contains(position))
            {
                position = worldBound.center;
            }

            ShowGradientPicker(position);

            evt.StopPropagation();
        }

        public override void SetValueWithoutNotify(Gradient newValue)
        {
            base.SetValueWithoutNotify(newValue);

            _valueNull = newValue == null;
            UpdateGradientTexture();
        }

        protected override void UpdateMixedValueContent()
        {
            if (showMixedValue)
            {
                _gradientInput.preview.style.backgroundImage = _mDefaultBackground;
                _gradientInput.preview.Add(mixedValueLabel);
            }
            else
            {
                UpdateGradientTexture();
                mixedValueLabel.RemoveFromHierarchy();
            }
        }
        
        public class GradientInput : VisualElement
        {
            private static Texture2D _checkerBoardTexture;

            public readonly VisualElement checkerBoard;
            public readonly VisualElement preview;
            
            public GradientInput()
            {
                checkerBoard = new Checkerboard(CheckerboardTheme.Dark);

                preview = new VisualElement()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = Length.Percent(100),
                    }
                };
                
                checkerBoard.Add(preview);
                Add(checkerBoard);
            }
        }
    }
}