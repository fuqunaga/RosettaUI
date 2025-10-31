using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public  static class UIToolkitUtility
    {
        private delegate Focusable GetLeafFocusedElementDelegate(FocusController focusController);
        
        private static GetLeafFocusedElementDelegate _focusControllerGetLeafFocusedElementDelegate;
#if UNITY_2023_1_OR_NEWER
        private static PropertyInfo _baseBoolFieldAcceptClicksIfDisabledPropertyInfo;
#else
        private static FieldInfo _baseBoolFieldClickableFieldInfo;
        private static PropertyInfo _clickableAcceptClicksIfDisabledPropertyInfo;
#endif
        
        public static bool WillUseKeyInput(IPanel panel)
        {
            if (_focusControllerGetLeafFocusedElementDelegate == null)
            {
                var methodInfo = typeof(FocusController).GetMethod("GetLeafFocusedElement", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(methodInfo);

                _focusControllerGetLeafFocusedElementDelegate ??= (GetLeafFocusedElementDelegate)Delegate.CreateDelegate(typeof(GetLeafFocusedElementDelegate), methodInfo);
                Assert.IsNotNull(_focusControllerGetLeafFocusedElementDelegate);    
            }
            
            var focusController = panel?.focusController;
            if (focusController == null) return false;
            
            var focusable = _focusControllerGetLeafFocusedElementDelegate(focusController);
            return focusable is ITextEdition { isReadOnly: false};
        }

        public static void SetAcceptClicksIfDisabled(BaseBoolField baseBoolField, bool flag = true)
        {
#if UNITY_2023_1_OR_NEWER
            _baseBoolFieldAcceptClicksIfDisabledPropertyInfo ??= typeof(BaseBoolField).GetProperty("acceptClicksIfDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
            _baseBoolFieldAcceptClicksIfDisabledPropertyInfo?.SetValue(baseBoolField, flag);
#else
            _baseBoolFieldClickableFieldInfo ??= typeof(BaseBoolField).GetField("m_Clickable", BindingFlags.NonPublic | BindingFlags.Instance);
            _clickableAcceptClicksIfDisabledPropertyInfo ??= typeof(Clickable).GetProperty("acceptClicksIfDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
            
            var baseBoolFieldClickable = _baseBoolFieldClickableFieldInfo?.GetValue(baseBoolField);
            _clickableAcceptClicksIfDisabledPropertyInfo?.SetValue(baseBoolFieldClickable, flag);
#endif
        }
    }
}