using System.Collections.Generic;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class PopupMenuUtility
    {
        public static void Show(IEnumerable<MenuItem> menuItems, Vector2 position, VisualElement targetElement)
        {
            var menu = DropDownMenuGenerator.Generate(
                menuItems,
                new Rect() { position = position },
                targetElement);
                    
            if (menu is GenericDropdownMenu gdm)
            {
                gdm.AddBoxShadow();
            }
        }
    }
}