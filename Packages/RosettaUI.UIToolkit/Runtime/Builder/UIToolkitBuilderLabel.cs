using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        VisualElement Build_Label(Element element)
        {
            var ve = new Label();
            Bind_Label(ve, element);
            return ve;
        }
        
        void SetupFieldLabel<T, TElementValue>(BaseField<T> field, ReadOnlyFieldElement<TElementValue> fieldBaseElement)
        {
            var labelElement = fieldBaseElement.label;
            if (labelElement != null)
            {
                field.ListenLabel(labelElement);

                SetupUIObj(labelElement, field.labelElement);
            }
        }

        private bool Bind_Label(VisualElement ve, Element element)
        {
            if (ve is not Label label || element is not LabelElement labelElement) return false;
            var viewBridge = labelElement.GetViewBridge();
            viewBridge.UnsubscribeAll();
            viewBridge.SubscribeValueOnUpdateCallOnce(label);
            return true;

        }
    }
}