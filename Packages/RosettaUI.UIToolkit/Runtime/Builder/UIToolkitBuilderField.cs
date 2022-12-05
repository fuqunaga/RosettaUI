using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_TextField(Element element, VisualElement visualElement)
        {
            if (element is not TextFieldElement textFieldElement || visualElement is not TextField textField) return false;

            Bind_Field(textFieldElement, textField, true);

            if (textFieldElement.IsMultiLine != textField.multiline)
            {
                textField.multiline = textFieldElement.IsMultiLine;
            }

            return true;
        }
   

        private bool Bind_ColorField(Element element, VisualElement visualElement)
        {
            if (element is not FieldBaseElement<Color> colorElement || visualElement is not ColorField colorField) return false;

            Bind_Field(colorElement, colorField, true);
            
            colorField.showColorPickerFunc += ShowColorPicker;
            element.GetViewBridge().onUnsubscribe += () => colorField.showColorPickerFunc -= ShowColorPicker;

            return true;
            
            
            void ShowColorPicker(Vector2 pos, UnityInternalAccess.ColorField target)
            {
                ColorPicker.Show(pos, target, colorField.value, color => colorField.value = color);
            }
        }

        public TField Build_Field<TValue, TField>(Element element)
            where TField : BaseField<TValue>, new()
        {
            return Build_Field<TValue, TField>(element, Bind_Field<TValue, TField>);
        }

        private TField Build_Field<TValue, TField>(Element element, bool labelEnable)
            where TField : BaseField<TValue>, new()
        {
            var field = CreateField<TValue, TField>();
            Bind_Field((FieldBaseElement<TValue>) element, field, labelEnable);

            return field;
        }

        private static TField Build_Field<TValue, TField>(Element element, Func<Element, VisualElement, bool> bindMethod)
            where TField : BaseField<TValue>, new()
        {
            var field = CreateField<TValue, TField>();
            var success = bindMethod(element, field);
            Assert.IsTrue(success);

            return field;
        }
        
        private static TField CreateField<TValue, TField>()
            where TField : BaseField<TValue>, new()
        {
            var field = new TField();

            // ラベルのChangeEventを潰しておく
            // fieldが BaseField<string> だとラベルのChangeEventを受け取ってしまうのでそれを止める
            if (typeof(TValue) == typeof(string))
            {
                field.labelElement.RegisterValueChangedCallback(evt => evt.StopPropagation());
            }

            return field;
        }

        private bool Bind_Field<TValue, TField>(Element element, VisualElement visualElement)
            where TField : BaseField<TValue>, new()
        {
            if (element is not FieldBaseElement<TValue> fieldBaseElement || visualElement is not TField field) return false;
            
            Bind_Field(fieldBaseElement, field, true);

            return true;
        }

        private void Bind_Field<TValue, TField>(FieldBaseElement<TValue> element, TField field, bool labelEnable)
            where TField : BaseField<TValue>, new()
        {
            element.Bind(field);
            
            if (labelEnable)
            {
                Bind_ExistingLabel(element.Label, field.labelElement, str => field.label = str);
            }
            else
            {
                //　Bind時以前のVisualElementのラベルを消しとく
                field.label = null;
            }
        }
    }
}