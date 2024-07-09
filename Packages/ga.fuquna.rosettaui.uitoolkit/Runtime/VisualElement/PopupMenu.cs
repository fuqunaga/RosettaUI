using System;
using System.Collections.Generic;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// 右クリックでポップアップメニューを出す空のVisualElement
    /// 見た目などの設定は子供に別のVisualElementを追加する
    /// </summary>
    public class PopupMenu : VisualElement
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
        
        public int ButtonIndex { get; set; } = 1;
        public Func<IEnumerable<MenuItem>> CreateMenuItems { get; set; }
        
        public PopupMenu()
        {
            RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != ButtonIndex) return;
                
                var menuItems = CreateMenuItems?.Invoke();
                if (menuItems == null) return;
                
                Show(menuItems, evt.position, this);

                evt.StopPropagationAndFocusControllerIgnoreEvent();
            });
        }
    }
}