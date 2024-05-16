using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public  static class UIToolkitUtility
    {
        public static bool WillUseKeyInput(IPanel panel)
        {
#if UNITY_2022_1_OR_NEWER
            // TODO
            return false;
#else
#if UNITY_2022_1_OR_NEWER
            // refs: TextInputBaseField.hasFocus, TextElement.hasFocus 
            var element = panel?.focusController?.GetLeafFocusedElement();
            return (element is TextElement textElement) &&
                   (
                       // Single line
                       (textElement.parent?.name == TextInputBaseField<string>.textInputUssName) ||
                       
                       // Multi line
                       // ref: TextInputBase.SetMultiline()
                       (textElement.parent?.parent?.name == TextInputBaseField<string>.textInputUssName)
                   );
#else
            // refs: TextInputBase.hasFocus
            var element = panel?.focusController?.GetLeafFocusedElement();
            return element is ITextInputField {hasFocus: true};
#endif
#endif
        }

        public static void SetAcceptClicksIfDisabled(BaseBoolField baseBoolField, bool flag = true)
        {
#if !UNITY_2022_1_OR_NEWER
            baseBoolField.m_Clickable.acceptClicksIfDisabled = flag;
#endif
        }

        public static void RegisterCallbackIncludeDisabled<TEventType>(VisualElement ve, EventCallback<TEventType> callback)
            where TEventType : EventBase<TEventType>, new()
        {
#if !UNITY_2022_1_OR_NEWER
            ve.RegisterCallback(callback, InvokePolicy.IncludeDisabled);
#endif
        }
    }
}