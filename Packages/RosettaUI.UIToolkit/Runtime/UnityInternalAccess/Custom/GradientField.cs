using System;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    using RosettaUI.Builder;
    
    public class GradientField :  BaseField<Gradient>
    {
        public new static readonly string ussClassName = "rosettaui-gradient-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";
        // public static readonly string contentUssClassName = ussClassName + "__content";

        static readonly GradientColorKey k_WhiteKeyBegin = new GradientColorKey(Color.white, 0);
        static readonly GradientColorKey k_WhiteKeyEnd = new GradientColorKey(Color.white, 1);
        static readonly GradientAlphaKey k_AlphaKeyBegin = new GradientAlphaKey(1, 0);
        static readonly GradientAlphaKey k_AlphaKeyEnd = new GradientAlphaKey(1, 1);

        public event Action<Vector2, GradientField> showGradientPickerFunc;

        private bool _valueNull;

        readonly Background m_DefaultBackground = new Background();

        /// <summary>
        /// Constructor.
        /// </summary>
        public GradientField()
            : this(null) {}

        /// <summary>
        /// Constructor.
        /// </summary>
        public GradientField(string label) : base(label, new GradientInput())
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            
            visualInput.AddToClassList(inputUssClassName);
            visualInput.RegisterCallback<ClickEvent>(OnClickInput);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);

            // rawValue = new Gradient();
        }
        
        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);

            if ((evt as MouseDownEvent)?.button == (int)MouseButton.LeftMouse)
            {
                var mde = (MouseDownEvent)evt;
                if (visualInput.ContainsPoint(visualInput.WorldToLocal(mde.mousePosition)))
                {
                    // showGradientPicker = true;
                    ShowGradientPicker(mde.mousePosition);
                    evt.StopPropagation();
                }
            }

        }
        
        void ShowGradientPicker(Vector2 position)
        {
            showGradientPickerFunc?.Invoke(position, this);
        }

        internal override void OnViewDataReady()
        {
            base.OnViewDataReady();
            UpdateGradientTexture();
        }

        void UpdateGradientTexture()
        {
            if (_valueNull || showMixedValue)
            {
                visualInput.style.backgroundImage = m_DefaultBackground;
            }
            else
            {
                Texture2D gradientTexture = GradientPickerHelper.GenerateGradientPreview(value, resolvedStyle.backgroundImage.texture);
                visualInput.style.backgroundImage = gradientTexture;

                IncrementVersion(VersionChangeType.Repaint); // since the Texture2D object can be reused, force dirty because the backgroundImage change will only trigger the Dirty if the Texture2D objects are different.
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
                visualInput.style.backgroundImage = m_DefaultBackground;
                visualInput.Add(mixedValueLabel);
            }
            else
            {
                UpdateGradientTexture();
                mixedValueLabel.RemoveFromHierarchy();
            }
        }

        public class GradientInput : VisualElement
        {
            public GradientInput()
            {
                pickingMode = PickingMode.Ignore;

            }
        }
    }
}