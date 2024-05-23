using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public static class DropDownMenuGenerator
    {
        public static object Generate(IEnumerable<MenuItem> menuItems, Rect position, VisualElement targetElement = null, bool anchored = false)
        {
            if (menuItems == null) return null;
#if UNITY_2023_1_OR_NEWER
            // refs: BasePopupField
            // https://github.com/Unity-Technologies/UnityCsReference/blob/c84064be69f20dcf21ebe4a7bbc176d48e2f289c/ModuleOverrides/com.unity.ui/Core/Controls/BasePopupField.cs#L206
            var isPlayer = targetElement?.panel.contextType == ContextType.Player;

            // TODO: On editor
            
            // see: DropdownUtility, EditorDelegateRegistration, GenericOSMenu
            // var menu = isPlayer
            //     ? new GenericDropdownMenu()
            //     : new UnityEditor.GenericMenu();
            var menu = new GenericDropdownMenu();

            foreach (var item in menuItems)
            {
                if (item.isEnable) menu.AddItem(item.name, item.isChecked, item.action);
                else menu.AddDisabledItem(item.name, item.isChecked);
            }

            menu.DropDown(position, targetElement, anchored);
#else
            // refs: BasePopupField
            // https://github.com/Unity-Technologies/UnityCsReference/blob/c84064be69f20dcf21ebe4a7bbc176d48e2f289c/ModuleOverrides/com.unity.ui/Core/Controls/BasePopupField.cs#L206
            var isPlayer = targetElement?.elementPanel.contextType == ContextType.Player;
            
            var menu = isPlayer
                ? new GenericDropdownMenu()
                : DropdownUtility.CreateDropdown();

            foreach (var item in menuItems)
            {
                if (item.isEnable) menu.AddItem(item.name, item.isChecked, item.action);
                else menu.AddDisabledItem(item.name, item.isChecked);
            }

            menu.DropDown(position, targetElement, anchored);
#endif
            return menu;
        }
    }
}