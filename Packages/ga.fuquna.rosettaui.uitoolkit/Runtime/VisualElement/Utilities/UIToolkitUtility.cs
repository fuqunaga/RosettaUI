using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public  static class UIToolkitUtility
    {
        private static MethodInfo _focusControllerGetLeafFocusedElementMethodInfo;
#if UNITY_2023_1_OR_NEWER
        private static PropertyInfo _baseBoolFieldAcceptClicksIfDisabledPropertyInfo;
#endif
        
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
#if UNITY_2023_1_OR_NEWER
            _baseBoolFieldAcceptClicksIfDisabledPropertyInfo ??= typeof(BaseBoolField).GetProperty("acceptClicksIfDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
            _baseBoolFieldAcceptClicksIfDisabledPropertyInfo?.SetValue(baseBoolField, flag);
#else
            baseBoolField.m_Clickable.acceptClicksIfDisabled = flag;
#endif
        }
    }
}