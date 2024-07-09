using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class MoreVertMenuButton : Button
    {
        public const string UssClassName = "rosettaui-morevert-menu-button";

        public int ButtonIndex { get; set; } = 1;
        public Func<IEnumerable<MenuItem>> CreateMenuItems { get; set; }

        public MoreVertMenuButton()
        {
            AddToClassList(UssClassName);

            clickable = new Clickable(OnClick);
        }

        private void OnClick(EventBase evtBase)
        {
            Vector2? position = evtBase switch
            {
                PointerUpEvent pointerUpEvent => pointerUpEvent.position,
                MouseUpEvent mouseUpEvent => mouseUpEvent.mousePosition,
                _ => null
            };
            
            if (position == null) return;
            
            var menuItems = CreateMenuItems?.Invoke();
            if (menuItems == null) return;
                
            PopupMenu.Show(menuItems, position.Value, this);

            evtBase.StopPropagationAndFocusControllerIgnoreEvent();
        }
    }
}