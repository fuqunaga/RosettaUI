using UnityEngine;
using UnityEngine.UIElements;
using RosettaUI.UIToolkit.UnityInternalAccess;

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

        private bool Bind_GradientField(Element element, VisualElement visualElement)
        {
            if(element is not FieldBaseElement<Gradient> gradientElement || visualElement is not GradientField gradientField) return false;
            
            Bind_Field(gradientElement, gradientField, true);
            
            gradientField.showGradientPickerFunc += ShowGradientPicker;
            element.GetViewBridge().onUnsubscribe += () => gradientField.showGradientPickerFunc -= ShowGradientPicker;
            
            return true;
            
            void ShowGradientPicker(Vector2 pos, UnityInternalAccess.GradientField target)
            {
                GradientEditor.Show(pos, target, gradientField.value, gradient => gradientField.value = gradient);
            }
        }
        
        public bool Bind_Field<TValue, TField>(Element element, VisualElement visualElement)
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

            if (field is TextInputBaseField<TValue> textInputBaseField)
            {
                textInputBaseField.isDelayed = element.Option.delayInput;
            }
            
            if (labelEnable)
            {
                Bind_FieldLabel(element, field);
            }
            else
            {
                //　Bind時以前のVisualElementのラベルを消しとく
                field.label = null;
            }
        }
    }
}