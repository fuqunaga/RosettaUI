using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public  static class UIToolkitUtility
    {
        public static bool WillUseKeyInput(IPanel panel)
        {
            // refs: TextInputBase.hasFocus
            var element = panel?.focusController?.GetLeafFocusedElement();
            return element is ITextInputField {hasFocus: true};
        }

        public static void SetAcceptClicksIfDisabled(BaseBoolField baseBoolField, bool flag = true)
        {
            baseBoolField.m_Clickable.acceptClicksIfDisabled = flag;
        }

        public static void RegisterCallbackIncludeDisabled<TEventType>(VisualElement ve, EventCallback<TEventType> callback)
            where TEventType : EventBase<TEventType>, new()
        {
            ve.RegisterCallback(callback, InvokePolicy.IncludeDisabled);
        }
    }
}