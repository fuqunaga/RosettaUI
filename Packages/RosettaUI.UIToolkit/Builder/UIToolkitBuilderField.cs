using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static VisualElement Build_IntField(Element element)
        {
            var intField = Build_Field<int, IntegerField>(element);
            intField.isUnsigned = ((IntFieldElement) element).isUnsigned;

            return intField;
        }


        private static VisualElement Build_ColorField(Element element)
        {
            var colorField = Build_Field<Color, ColorField>(element);

            colorField.showColorPickerFunc += (pos, target) =>
            {
                ColorPicker.Show(pos, target, colorField.value, color => colorField.value = color);
            };

            return colorField;
        }

        private static TField Build_Field<T, TField>(Element element)
            where TField : BaseField<T>, new()
        {
            return Build_Field<T, TField>(element, true);
        }
        
        private static TField Build_Field<T, TField>(Element element, bool labelEnable)
            where TField : BaseField<T>, new()
        {
            var fieldBaseElement = (FieldBaseElement<T>) element;

            var field = new TField();
            field.Bind(fieldBaseElement);

            if (labelEnable)
            {
                SetupLabelCallback(field, fieldBaseElement);
            }

            return field;
        }
    }
}