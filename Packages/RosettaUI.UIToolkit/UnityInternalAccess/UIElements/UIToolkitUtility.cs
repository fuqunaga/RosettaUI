#define AvoidInternal

using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.PackageInternal
{
    public  static class UIToolkitUtility
    {
        public static bool WillUseKeyInput(IPanel panel)
        {
#if !AvoidInternal
            // refs: TextInputBase.hasForus
            var element = panel?.focusController?.GetLeafFocusedElement();
            if (element is ITextInputField textInputField)
            {
                return textInputField.hasFocus;
            }
#endif

            return false;
        }
    }
}