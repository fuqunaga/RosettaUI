using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorFieldBase : PreviewBaseField<Color>
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once ConvertToConstant.Global
        public new static readonly string ussClassName = "rosettaui-color-field";

        protected ColorInput colorInput;

        public bool EnableAlpha
        {
            get => colorInput.DisplayAlpha;
            set => colorInput.DisplayAlpha = value;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColorFieldBase() : this(null) { }

        public ColorFieldBase(string label) : base(label, new ColorInput())
        {
            colorInput = this.Q<ColorInput>();
        }

        public override void SetValueWithoutNotify(Color color)
        {
            base.SetValueWithoutNotify(color);
            colorInput.SetColor(color);
        }

        protected override void ShowEditor(Vector3 position)
        {
            ColorPicker.Show(position, this, value, color => value = color, EnableAlpha);
        }


        // ReSharper disable once MemberCanBeProtected.Global
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public class ColorInput : VisualElement
        {
            public static readonly string ussFieldInputRGB = ussClassName + "__input-rgb";
            public static readonly string ussFieldInputAlpha = ussClassName + "__input-alpha";
            public static readonly string ussFieldInputAlphaContainer = ussClassName + "__input-alpha-container";

            public readonly VisualElement rgbField;
            public readonly VisualElement alphaField;
            public readonly VisualElement alphaFieldContainer;

            public bool DisplayAlpha
            {
                get => alphaFieldContainer.resolvedStyle.display != DisplayStyle.None;
                set => alphaFieldContainer.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            public ColorInput()
            {
                pickingMode = PickingMode.Ignore;
                
                rgbField = new VisualElement();
                rgbField.AddToClassList(ussFieldInputRGB);
                Add(rgbField);

                alphaFieldContainer = new VisualElement();
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
