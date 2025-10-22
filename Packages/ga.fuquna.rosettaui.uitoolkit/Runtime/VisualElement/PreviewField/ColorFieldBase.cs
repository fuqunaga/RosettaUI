using System.Diagnostics.CodeAnalysis;
using RosettaUI.UIToolkit.UndoSystem;
using RosettaUI.UndoSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorFieldBase : PreviewFieldBase<Color, ColorFieldBase.ColorInput>
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once ConvertToConstant.Global
        public new static readonly string ussClassName = "rosettaui-color-field";

        public bool EnableAlpha
        {
            get => inputField.DisplayAlpha;
            set => inputField.DisplayAlpha = value;
        }
        
        public ColorFieldBase() : this(null) { }

        public ColorFieldBase(string label) : base(label, new ColorInput())
        {
        }

        protected override void ShowEditor(Vector3 position)
        {
            var initialValue = value;
            
            ColorPicker.Show(position, this, initialValue,
                onColorChanged: color => value = color,
                onHide: OnHide,
                enableAlpha: EnableAlpha
            );

            return;

            void OnHide(bool isCancelled)
            {
                if (isCancelled)
                {
                    value = initialValue;
                }
                else if (initialValue != value)
                {
                    UndoUIToolkit.RecordBaseField(nameof(ColorFieldBase), this, initialValue, value);
                }
            }
        }

        public override void SetValueWithoutNotify(Color color)
        {
            base.SetValueWithoutNotify(color);
            inputField.SetColor(color);
        }

        
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
