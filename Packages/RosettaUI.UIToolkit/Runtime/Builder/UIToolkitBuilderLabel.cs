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
        
        private void SetupFieldLabel<T, TElementValue>(BaseField<T> field, ReadOnlyFieldElement<TElementValue> fieldBaseElement)
        {
            var labelElement = fieldBaseElement.Label;
            if (labelElement == null) return;

            labelElement.GetViewBridge().SubscribeValueOnUpdateCallOnce(str => field.label = str);
            SetupUIObj(labelElement, field.labelElement);
        }

        private bool Bind_Label(Element element, VisualElement visualElement)
        {
            if (visualElement is not Label label || element is not LabelElement labelElement) return false;
            labelElement.SubscribeValueOnUpdateCallOnce(label);
            
            return true;
        }
    }
}