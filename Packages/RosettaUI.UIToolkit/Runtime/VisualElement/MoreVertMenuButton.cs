using System;
using System.Collections.Generic;
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
            if (evtBase is not PointerUpEvent evt) return;
            
            var menuItems = CreateMenuItems?.Invoke();
            if (menuItems == null) return;
                
            PopupMenu.Show(menuItems, evt.position, this);

            evt.StopPropagationAndFocusControllerIgnoreEvent();
        }
    }
}