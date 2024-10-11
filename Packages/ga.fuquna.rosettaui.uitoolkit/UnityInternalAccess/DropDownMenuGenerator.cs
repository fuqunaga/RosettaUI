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
            // refs: DropdownUtility, EditorDelegateRegistration, GenericOSMenu
            var menu = new GenericDropdownMenu();
#else
            // refs: BasePopupField
            // https://github.com/Unity-Technologies/UnityCsReference/blob/c84064be69f20dcf21ebe4a7bbc176d48e2f289c/ModuleOverrides/com.unity.ui/Core/Controls/BasePopupField.cs#L206
            var isPlayer = targetElement?.elementPanel.contextType == ContextType.Player;

            IGenericMenu menu;
            if (isPlayer)
            {
                // Unity2022.3でスクロールバーが表示されてしまう（バグ？）のであらかじめ広めの幅を確保しておく
                var genericMenu = new GenericDropdownMenu();
                genericMenu.contentContainer.style.width = 200;

                menu = genericMenu;
            }
            
            // GenericDropdownMenu だとエディターでエラーになる場合があるのでDropdownUtility経由でOSのメニューを使用する
            // RosettaUIEditorWindowExample > MiscExample > UI.Popup() でエラーになる
            // Unity2022.3
            else
            {
                menu = DropdownUtility.CreateDropdown();
            }
#endif

            foreach (var item in menuItems)
            {
                if ( item == MenuItem.Separator)
                {
                    menu.AddSeparator("");
                    continue;
                }
                
                if (item.isEnable)
                {
                    menu.AddItem(item.name, item.isChecked, item.action);
                }
                else
                {
                    menu.AddDisabledItem(item.name, item.isChecked);
                }
            }

            menu.DropDown(position, targetElement, anchored);
            return menu;
        }
    }
}