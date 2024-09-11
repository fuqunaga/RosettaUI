using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI
{
    public interface IPopupMenuImplement
    {
        void Show(IEnumerable<MenuItem> menuItems, Vector2 screenPosition, Element targetElement);
    }
    
    public static class PopupMenu
    {
        public static IPopupMenuImplement Implement { get; set; }
        
        public static void Show(IEnumerable<MenuItem> menuItems, Vector2 position, Element targetElement)
        {
            if ( Implement == null)
            {
                Debug.LogWarning("PopupMenu.Implement is not set.");
                return;
            }
                
            Implement.Show(menuItems, position, targetElement);
        }
    }
}