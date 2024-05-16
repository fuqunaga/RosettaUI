using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public static class DropDownMenuGenerator
    {
        public static object Generate(IEnumerable<MenuItem> menuItems, Rect position, VisualElement targetElement = null, bool anchored = false)
        {
#if UNITY_2021_2_OR_NEWER
            // TODO: Implement for Unity 2022.1 or newer
            return null;
#else
            if (menuItems == null) return null;

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

            return menu;
#endif
        }
    }
}