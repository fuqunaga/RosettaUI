using UnityEngine;
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

        private bool Bind_GradientField(Element element, VisualElement visualElement)
        {
            if(element is not FieldBaseElement<Gradient> gradientElement || visualElement is not GradientField gradientField) return false;
            
            Bind_Field(gradientElement, gradientField, true);
            
            gradientField.showGradientPickerFunc += ShowGradientPicker;
            element.GetViewBridge().onUnsubscribe += () => gradientField.showGradientPickerFunc -= ShowGradientPicker;
            
            return true;
            
            void ShowGradientPicker(Vector2 pos, GradientField target)
            {
                GradientEditor.Show(pos, target, gradientField.value, gradient => gradientField.value = gradient);
            }
        }
        
        private bool Bind_AnimationCurveField(Element element, VisualElement visualElement)
        {
            if(element is not FieldBaseElement<AnimationCurve> animationCurveElement || visualElement is not AnimationCurveField animationCurveField) return false;
            
            Bind_Field(animationCurveElement, animationCurveField, true);
            
            animationCurveField.showAnimationCurveEditorFunc += ShowAnimationCurveEditor;
            element.GetViewBridge().onUnsubscribe += () => animationCurveField.showAnimationCurveEditorFunc -= ShowAnimationCurveEditor;
            
            return true;
            
            void ShowAnimationCurveEditor(Vector2 pos, AnimationCurveField target)
            {
                AnimationCurveEditor.AnimationCurveEditor.Show(pos, target, animationCurveField.value, gradient => animationCurveField.value = gradient);
            }
        }
        
        public bool Bind_Field<TValue, TField>(Element element, VisualElement visualElement)
            where TField : BaseField<TValue>, new()
        {
            if (element is not FieldBaseElement<TValue> fieldBaseElement ||  visualElement.GetType() != typeof(TField)) return false;

            Bind_Field(fieldBaseElement, (TField)visualElement, true);

            return true;
        }

        private void Bind_Field<TValue, TField>(FieldBaseElement<TValue> element, TField field, bool labelEnable)
            where TField : BaseField<TValue>
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