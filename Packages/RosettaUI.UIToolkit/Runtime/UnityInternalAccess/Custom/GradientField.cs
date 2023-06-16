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
        /// <summary>
        /// The <see cref="Gradient"/> currently being exposed by the field.
        /// </summary>
        /// <remarks>
        /// Note that changing this will not trigger a change event to be sent.
        /// </remarks>
        public override Gradient value
        {
            get
            {
                if (_valueNull) return null;

                return GradientCopy(rawValue);
            }
            set
            {
                if (value != null || !_valueNull)  // let's not reinitialize an initialized gradient
                {
                    using (ChangeEvent<Gradient> evt = ChangeEvent<Gradient>.GetPooled(rawValue, value))
                    {
                        // evt.elementTarget = this;
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                        // NotifyPropertyChanged(valueProperty);
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

            rawValue = new Gradient();
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
                Texture2D gradientTexture = GradientPickerHelper.GenerateGradientPreview(rawValue, resolvedStyle.backgroundImage.texture);
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
        
        bool CompareGradient(Gradient a, Gradient b)
        {
            if (a == null || b == null)
                return false;

            if (a.colorKeys.Length != b.colorKeys.Length)
                return false;
            if (a.alphaKeys.Length != b.alphaKeys.Length)
                return false;
            if (a.mode != b.mode)
                return false;
            
            for(int i = 0; i < a.colorKeys.Length; ++i)
            {
                if (a.colorKeys[i].color != b.colorKeys[i].color)
                    return false;
                if (Math.Abs(a.colorKeys[i].time - b.colorKeys[i].time) > float.Epsilon)
                    return false;
            }
            
            for(int i = 0; i < a.alphaKeys.Length; ++i)
            {
                if (Math.Abs(a.alphaKeys[i].alpha - b.alphaKeys[i].alpha) > float.Epsilon)
                    return false;
                if (Math.Abs(a.alphaKeys[i].time - b.alphaKeys[i].time) > float.Epsilon)
                    return false;
            }

            return true;
        }
        
        public override void SetValueWithoutNotify(Gradient newValue)
        {
            _valueNull = newValue == null;
            bool isChange = !CompareGradient(rawValue, newValue);
            if (newValue != null)
            {
                rawValue.colorKeys = newValue.colorKeys;
                rawValue.alphaKeys = newValue.alphaKeys;
                rawValue.mode = newValue.mode;
            }
            else // restore the internal gradient to the default state.
            {
                rawValue.colorKeys = new[] { k_WhiteKeyBegin, k_WhiteKeyEnd };
                rawValue.alphaKeys = new[] { k_AlphaKeyBegin, k_AlphaKeyEnd };
                rawValue.mode = GradientMode.Blend;
            }

            if (isChange)
            {
                UpdateGradientTexture();
            }
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