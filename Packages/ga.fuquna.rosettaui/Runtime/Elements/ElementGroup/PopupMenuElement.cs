using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine.UIElements;

namespace RosettaUI
{
    public class PopupMenuElement : ElementGroup
    {
        public Func<IEnumerable<MenuItem>> CreateMenuItems { get; }
        public MouseButton MouseButton { get; set; }

        public PopupMenuElement(Element element, Func<IEnumerable<MenuItem>> createMenuItems, MouseButton mouseButton = MouseButton.RightMouse) : base(new[] {element})
        {
            CreateMenuItems = createMenuItems;
            MouseButton = mouseButton;
        }
    }
}