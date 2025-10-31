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
        public Func<IEnumerable<IMenuItem>> CreateMenuItems { get; set; }

        public MoreVertMenuButton()
        {
            AddToClassList(UssClassName);

            clickable = new Clickable(OnClick);
        }

        private void OnClick(EventBase evtBase)
        {
            if (!evtBase.TryGetPosition(out var position))
            {
                return;
            }
            
            var menuItems = CreateMenuItems?.Invoke();
            if (menuItems == null) return;
                
            PopupMenuUtility.Show(menuItems, position, this);

            evtBase.StopPropagationAndFocusControllerIgnoreEvent();
        }
    }
}