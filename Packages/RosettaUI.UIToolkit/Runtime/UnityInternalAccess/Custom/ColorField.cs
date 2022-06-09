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
            visualInput.RegisterCallback<ClickEvent>(OnClick);
        }

        public override void SetValueWithoutNotify(Color color)
        {
            base.SetValueWithoutNotify(color);
            colorInput.SetColor(color);
        }

        private void OnClick(ClickEvent evt)
        {
            showColorPickerFunc?.Invoke(evt.position, this);
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
