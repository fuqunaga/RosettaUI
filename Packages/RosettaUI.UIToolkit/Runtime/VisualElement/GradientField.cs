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

        private readonly Background m_DefaultBackground = new();

        private GradientInput _gradientInput;

        public override Gradient value
        {
            get
            {
                if (_valueNull) return null;

                return GradientCopy(rawValue);
            }
            set
            {
                if (value != null || !_valueNull) // let's not reinitialize an initialized gradient
                {
                    using (ChangeEvent<Gradient> evt = ChangeEvent<Gradient>.GetPooled(rawValue, value))
                    {
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                    }
                }
            }
        }

        internal static Gradient GradientCopy(Gradient other)
        {
            Gradient gradientCopy = new Gradient();
            gradientCopy.colorKeys = other.colorKeys;
            gradientCopy.alphaKeys = other.alphaKeys;
            gradientCopy.mode = other.mode;
            return gradientCopy;
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
            if (_valueNull || showMixedValue)
            {
                _gradientInput.style.backgroundImage = m_DefaultBackground;
            }
            else
            {
                _gradientInput.style.backgroundImage =
                    GradientHelper.GenerateGradientPreview(value, _gradientInput.style.backgroundImage.value.texture);
// #if !UNITY_2023_1_OR_NEWER
//                 IncrementVersion(VersionChangeType.Repaint); // since the Texture2D object can be reused, force dirty because the backgroundImage change will only trigger the Dirty if the Texture2D objects are different.
// #endif
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
            if (newValue != null)
            {
                value.colorKeys = newValue.colorKeys;
                value.alphaKeys = newValue.alphaKeys;
                value.mode = newValue.mode;
            }
            else // restore the internal gradient to the default state.
            {
                value.colorKeys = new[] { k_WhiteKeyBegin, k_WhiteKeyEnd };
                value.alphaKeys = new[] { k_AlphaKeyBegin, k_AlphaKeyEnd };
                value.mode = GradientMode.Blend;
            }

            UpdateGradientTexture();
        }

        protected override void UpdateMixedValueContent()
        {
            if (showMixedValue)
            {
                _gradientInput.style.backgroundImage = m_DefaultBackground;
                _gradientInput.Add(mixedValueLabel);
            }
            else
            {
                UpdateGradientTexture();
                mixedValueLabel.RemoveFromHierarchy();
            }
        }

        public class GradientInput : VisualElement
        {
        }
    }
}