using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class ColorField : BaseField<Color>
    {
        public new static readonly string ussClassName = "rosettaui-color-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";
        
        protected ColorInput colorInput => (ColorInput)visualInput;

        public event Action<Vector2, ColorField> showColorPickerFunc;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColorField() : this(null) { }

        public ColorField(string label) : base(label, new ColorInput())
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            
            visualInput.AddToClassList(inputUssClassName);
            visualInput.RegisterCallback<ClickEvent>(OnClickInput);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
        }

        public override void SetValueWithoutNotify(Color color)
        {
            base.SetValueWithoutNotify(color);
            colorInput.SetColor(color);
        }

        private void OnClickInput(ClickEvent evt)
        {
            ShowColorPicker(evt.position);
            
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
            
            ShowColorPicker(position);
            
            evt.StopPropagation();
        }

        private void ShowColorPicker(Vector2 position)
        {
            showColorPickerFunc?.Invoke(position, this);
        }
        

        public class ColorInput : VisualElement
        {
            public VisualElement rgbField;
            public VisualElement alphaField;

            static readonly string ussFieldInputRGB = ussClassName + "__input-rgb";
            static readonly string ussFieldInputAlpha = ussClassName + "__input-alpha";
            static readonly string ussFieldInputAlphaContainer = ussClassName + "__input-alpha-container";

            public ColorInput()
            {
                pickingMode = PickingMode.Ignore;
                
                rgbField = new VisualElement();
                rgbField.AddToClassList(ussFieldInputRGB);
                Add(rgbField);

                var alphaFieldContainer = new VisualElement();
                alphaFieldContainer.AddToClassList(ussFieldInputAlphaContainer);
                Add(alphaFieldContainer);

                alphaField = new VisualElement();
                alphaField.AddToClassList(ussFieldInputAlpha);
                alphaFieldContainer.Add(alphaField);
            }

            public void SetColor(Color color)
            {
                rgbField.style.backgroundColor = new Color(color.r, color.g, color.b, 1f);
                alphaField.style.width = Length.Percent(color.a * 100f);
            }
        }
    }
}
