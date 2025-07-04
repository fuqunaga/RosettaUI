using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveField : BaseField<AnimationCurve>
    {
        public new static readonly string ussClassName = "rosettaui-animation-curve-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        public event Action<Vector2, AnimationCurveField> showAnimationCurveEditorFunc = delegate { };

        private bool _valueNull;
        private readonly Background _defaultBackground = new();
        private readonly AnimationCurveInput _curveInput;

        public override AnimationCurve value
        {
            get => _valueNull ? null : AnimationCurveHelper.Clone(rawValue);
            set
            {
                if (value != null || !_valueNull)
                {
                    using var evt = ChangeEvent<AnimationCurve>.GetPooled(rawValue, value);
                    evt.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(evt);
                }
            }
        }

        public AnimationCurveField() : this(null)
        {
        }

        public AnimationCurveField(string label) : base(label, new AnimationCurveInput())
        {
            _curveInput = this.Q<AnimationCurveInput>();
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);

            _curveInput.AddToClassList(inputUssClassName);
            _curveInput.RegisterCallback<ClickEvent>(OnClickInput);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
        }

        private void ShowAnimationCurveEditor(Vector2 position)
        {
            showAnimationCurveEditorFunc.Invoke(position, this);
        }

        private void UpdateAnimationCurveTexture()
        {
            var preview = _curveInput.Preview;

            if (_valueNull || showMixedValue)
            {
                preview.style.backgroundImage = _defaultBackground;
            }
            else
            {
                var width = Mathf.CeilToInt(preview.CalcWidthPixelOnScreen());
                var height = Mathf.CeilToInt(preview.CalcHeightPixelOnScreen());
                
                if (width <= 0 || height <= 0)
                {
                    return;
                }

                var texture = preview.style.backgroundImage.value.renderTexture;
                texture = AnimationCurveHelper.GenerateAnimationCurvePreview(rawValue, texture, width, height);

                preview.style.backgroundImage = Background.FromRenderTexture(texture);
            }
        }

        private void OnClickInput(ClickEvent evt)
        {
            ShowAnimationCurveEditor(evt.position);

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

            ShowAnimationCurveEditor(position);

            evt.StopPropagation();
        }

        public override void SetValueWithoutNotify(AnimationCurve newValue)
        {
            base.SetValueWithoutNotify(newValue);

            _valueNull = newValue == null;
            UpdateAnimationCurveTexture();
        }

        protected override void UpdateMixedValueContent()
        {
            if (showMixedValue)
            {
                _curveInput.Preview.style.backgroundImage = _defaultBackground;
                _curveInput.Preview.Add(mixedValueLabel);
            }
            else
            {
                UpdateAnimationCurveTexture();
                mixedValueLabel.RemoveFromHierarchy();
            }
        }

        public class AnimationCurveInput : VisualElement
        {
            public readonly VisualElement Preview;

            public AnimationCurveInput()
            {
                Preview = new VisualElement()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = Length.Percent(100),
                    }
                };

                Add(Preview);
            }
        }
    }
}