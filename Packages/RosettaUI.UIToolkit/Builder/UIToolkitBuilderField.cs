using System;
using RosettaUI.Reactive;
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
            return Build_Field<T, T, TField>(element,
                (field, v) => field.SetValueWithoutNotify(v),
                (v) => v);
        }

        private static TField Build_Field<TElementValue, TFieldValue, TField>(
            Element element,
            Action<TField, TElementValue> onElementValueChanged,
            Func<TFieldValue, TElementValue> fieldToElement
        )
            where TField : BaseField<TFieldValue>, new()
        {
            var fieldElement = (FieldBaseElement<TElementValue>) element;
            var field = CreateField(fieldElement, onElementValueChanged, fieldToElement);

            var labelElement = fieldElement.label;
            if (labelElement != null)
            {
                field.label = labelElement.Value;
                SetupLabelCallback(field.labelElement, labelElement);
            }

            return field;
        }

        static TField CreateField<T, TField>(FieldBaseElement<T> fieldBaseElement)
            where TField : BaseField<T>, new()
        {
            return CreateField<T, T, TField>(fieldBaseElement, (field, v) => field.SetValueWithoutNotify(v), (v) => v);
        }

        private static TField CreateField<TElementValue, TFieldValue, TField>(
            FieldBaseElement<TElementValue> fieldBaseElement,
            //Func<TElementValue, TFieldValue> elementToFieldValue,
            Action<TField, TElementValue> onElementValueChanged,
            Func<TFieldValue, TElementValue> fieldToElement
        )
            where TField : BaseField<TFieldValue>, new()
        {
            var field = new TField();
            fieldBaseElement.valueRx.SubscribeAndCallOnce(v => onElementValueChanged(field, v));
            field.RegisterValueChangedCallback(ev => fieldBaseElement.OnViewValueChanged(fieldToElement(ev.newValue)));

            return field;
        }


    }
}