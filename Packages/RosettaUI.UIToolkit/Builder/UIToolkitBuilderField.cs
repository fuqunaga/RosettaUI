using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_TextField(Element element)
        {
            var textFieldElement = (TextFieldElement) element;

            var textField = Build_Field<string, TextField>(element);
            if (textFieldElement.MultiLine)
            {
                textField.multiline = true;
            }

            return textField;
        }

        private VisualElement Build_ColorField(Element element)
        {
            var colorField = Build_Field<Color, ColorField>(element);

            colorField.showColorPickerFunc += (pos, target) =>
            {
                ColorPicker.Show(pos, target, colorField.value, color => colorField.value = color);
            };

            return colorField;
        }

        private TField Build_Field<T, TField>(Element element)
            where TField : BaseField<T>, new()
        {
            return Build_Field<T, TField>(element, true);
        }
        
        private TField Build_Field<T, TField>(Element element, bool labelEnable)
            where TField : BaseField<T>, new()
        {
            var fieldBaseElement = (FieldBaseElement<T>) element;

            var field = new TField();
            field.Bind(fieldBaseElement);

            if (labelEnable)
            {
                SetupFieldLabel(field, fieldBaseElement);
            }

            return field;
        }
    }
}