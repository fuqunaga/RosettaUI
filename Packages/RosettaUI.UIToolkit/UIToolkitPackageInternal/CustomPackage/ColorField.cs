using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorField : BaseField<Color>
    {
        ColorInput colorInput => (ColorInput)visualInput;

        /// <summary>
        /// Instantiates a <see cref="ColorField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ColorField, UxmlTraits> { }
        /// <summary>
        /// Defines <see cref="UxmlTraits"/> for the <see cref="ColorField"/>.
        /// </summary>
        public new class UxmlTraits : TextValueFieldTraits<float, UxmlFloatAttributeDescription> { }
        
        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public new static readonly string ussClassName = "rosettaui-color-field";
        /// <summary>
        /// USS class name of labels in elements of this type.
        /// </summary>
        public new static readonly string labelUssClassName = ussClassName + "__label";
        /// <summary>
        /// USS class name of input elements in elements of this type.
        /// </summary>
        public new static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColorField() : this((string)null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        public ColorField(string label)
            : base(label, new ColorInput())
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            visualInput.AddToClassList(inputUssClassName);

            //visualInput.RegisterCallback<ClickEvent>(OnClick);
        }

        public override void SetValueWithoutNotify(Color color)
        {
            base.SetValueWithoutNotify(color);
            colorInput.SetColor(color);
        }


        class ColorInput : VisualElement
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
                alphaField.style.width = alphaField.parent.resolvedStyle.width * color.a;
            }
        }
    }
}
