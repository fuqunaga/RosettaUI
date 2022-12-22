using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Toggle(Element element, VisualElement visualElement)
        {
            if (element is not ToggleElement toggleElement || visualElement is not Toggle toggle) return false;

            Bind_Field(toggleElement, toggle, !toggleElement.isLabelRight);

            // SetupFieldLabel()のtoggle.text版
            // toggle.labelはチェックボックスの左、toggle.textは右
            if (toggleElement.isLabelRight && toggleElement.Label is {} label)
            {
                label.GetViewBridge().SubscribeValueOnUpdateCallOnce(text => toggle.text = text);
            }
            
            return true;
        }
    }
}