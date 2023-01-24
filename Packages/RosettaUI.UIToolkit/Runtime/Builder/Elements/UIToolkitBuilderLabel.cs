using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        VisualElement Build_Label(Element element)
        {
            var ve = new Label();
            Bind_Label(element, ve);
            return ve;
        }

        private bool Bind_Label(Element element, VisualElement visualElement)
        {
            if (visualElement is not Label label || element is not LabelElement labelElement) return false;
            labelElement.SubscribeValueOnUpdateCallOnce(label);
            
            return true;
        }
        
        /// <summary>
        /// Fieldがもともと内包しているLabelとLabelElementをBindする
        /// </summary>
        private void Bind_FieldLabel<T, TElementValue>(ReadOnlyFieldElement<TElementValue> fieldBaseElement, BaseField<T> field)
        {
            Bind_ExistingLabel(fieldBaseElement.Label, field.labelElement, (str) => field.label = str);
        }
        
        /// <summary>
        /// 一部のVisualElementがもともと内包しているLabelとLabelElementをBindする
        /// LabelはアクセスできないがBaseBoolField.textなどのように値を渡せる場合もある
        /// </summary>
        private void Bind_ExistingLabel(LabelElement labelElement, Label label, Action<string> setValueToView)
        {
            if (labelElement == null)
            {
                setValueToView(null);
            }
            else
            {
                labelElement.GetViewBridge().SubscribeValueOnUpdateCallOnce(setValueToView);
                if (label != null)
                {
                    SetupUIObj(labelElement, label);
                }
            }
        }
    }
}