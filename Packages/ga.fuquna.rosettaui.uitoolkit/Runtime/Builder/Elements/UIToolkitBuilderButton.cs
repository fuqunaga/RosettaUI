using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static bool Bind_Button(Element element, VisualElement visualElement)
        {
            if (element is not ButtonElement buttonElement || visualElement is not Button button) return false;

            var viewBridge = buttonElement.GetViewBridge();
            
            // Button は TextElement を継承しているが SetValueWithoutNotify() で文字をセットしても正しく反応しない @Unity6000.0.2f1
            // Button.text に直接セット必要がある
            // - buttonElement.SubscribeValueOnUpdateCallOnce(button);
            viewBridge.SubscribeValueOnUpdateCallOnce(text => button.text = text);
            
            button.clicked += buttonElement.OnClick;
            viewBridge.onUnsubscribe += () => button.clicked -= buttonElement.OnClick;

            return true;
        }
    }
}