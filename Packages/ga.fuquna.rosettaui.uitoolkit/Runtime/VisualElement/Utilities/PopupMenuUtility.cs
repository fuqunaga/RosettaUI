using System.Collections.Generic;
using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class PopupMenuUIToolkit : IPopupMenuImplement
    {
        public void Show(IEnumerable<MenuItem> menuItems, Vector2 screenPosition, Element targetElement)
        {
            var visualElement = UIToolkitBuilder.Instance.GetUIObj(targetElement);
            if (visualElement == null)
            {
                Debug.LogWarning("PopupMenuUIToolkit.Show: targetElement is not a VisualElement.");
                return;
            }

            var position = screenPosition;
            if (visualElement.panel.contextType == ContextType.Player)
            {
                position = RuntimePanelUtils.ScreenToPanel(visualElement.panel, screenPosition);
            }

            PopupMenuUtility.Show(menuItems, position, visualElement);
        }
    }
    
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