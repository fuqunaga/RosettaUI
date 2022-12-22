using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static bool Bind_Button(Element element, VisualElement visualElement)
        {
            if (element is not ButtonElement buttonElement || visualElement is not Button button) return false;

            buttonElement.SubscribeValueOnUpdateCallOnce(button);
            
            var viewBridge = buttonElement.GetViewBridge();
            button.clicked += buttonElement.OnClick;
            viewBridge.onUnsubscribe += () => button.clicked -= buttonElement.OnClick;

            return true;
        }
    }
}