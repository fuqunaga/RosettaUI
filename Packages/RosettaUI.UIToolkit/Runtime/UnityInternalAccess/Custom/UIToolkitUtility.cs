using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public  static class UIToolkitUtility
    {
#if UNITY_2022_1_OR_NEWER
        
        private static MethodInfo _focusControllerGetLeafFocusedElementMethodInfo;
        private static PropertyInfo _baseBoolFieldAcceptClicksIfDisabledPropertyInfo;
        
        public static bool WillUseKeyInput(IPanel panel)
        {
            _focusControllerGetLeafFocusedElementMethodInfo ??= typeof(FocusController).GetMethod("GetLeafFocusedElement", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(_focusControllerGetLeafFocusedElementMethodInfo);
            
            var focusController = panel?.focusController;
            if (focusController == null) return false;
            
            var focusable = _focusControllerGetLeafFocusedElementMethodInfo.Invoke(focusController, null) as Focusable;

            return focusable is ITextEdition { isReadOnly: false};
        }

        public static void SetAcceptClicksIfDisabled(BaseBoolField baseBoolField, bool flag = true)
        {
            _baseBoolFieldAcceptClicksIfDisabledPropertyInfo ??= typeof(BaseBoolField).GetProperty("acceptClicksIfDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
            _baseBoolFieldAcceptClicksIfDisabledPropertyInfo?.SetValue(baseBoolField, flag);
        }

        public static void RegisterCallbackIncludeDisabled<TEventType>(VisualElement ve, EventCallback<TEventType> callback)
            where TEventType : EventBase<TEventType>, new()
        {

        }
        
#else
        
        public static bool WillUseKeyInput(IPanel panel)
        {
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
#endif
    }
}