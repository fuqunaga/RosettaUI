using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.PackageInternal
{
    public class ColorField : BaseField<Color>
    {
        #region Uxml

        /// <summary>
        /// Instantiates a <see cref="ColorField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ColorField, UxmlTraits> { }

        /// <summary>
        /// Defines <see cref="UxmlTraits"/> for the <see cref="ColorField"/>.
        /// </summary>
        public new class UxmlTraits : TextValueFieldTraits<float, UxmlFloatAttributeDescription> { }

        #endregion

        private new static readonly string ussClassName = "rosettaui-color-field";
        private new static readonly string labelUssClassName = ussClassName + "__label";
        private new static readonly string inputUssClassName = ussClassName + "__input";

        // TODO: internal
        //ColorInput colorInput => (ColorInput)visualInput;
        private ColorInput colorInput { get; } = new ColorInput();

        public event Action<Vector2, ColorField> showColorPickerFunc;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColorField() : this(null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        public ColorField(string label)
            : base(label, new ColorInput())
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            
            // TODO : internal
            /*
            visualInput.AddToClassList(inputUssClassName);
            visualInput.RegisterCallback<ClickEvent>(OnClick);
            */
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
                alphaField.style.width = Length.Percent(color.a * 100f);
            }
        }
    }
}
