using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine.UIElements;

namespace RosettaUI
{
    public class PopupMenuElement : ElementGroup
    {
        public Func<IEnumerable<IMenuItem>> CreateMenuItems { get; }
        public MouseButton MouseButton { get; set; }

        public PopupMenuElement(Element element, Func<IEnumerable<IMenuItem>> createMenuItems, MouseButton mouseButton = MouseButton.RightMouse) : base(new[] {element})
        {
            CreateMenuItems = createMenuItems;
            MouseButton = mouseButton;
        }
    }
}