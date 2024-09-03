using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine.UIElements;

namespace RosettaUI
{
    public class MenuItem
    {
        public static readonly MenuItem Separator = new();
        public static IEqualityComparer<MenuItem> NameComparer { get; } = new NameComparerImpl();
        
        public string name;
        public bool isChecked;
        public bool isEnable = true;
        public Action action;

        public MenuItem()
        {
        }

        public MenuItem(string name, Action action)
        {
            this.name = name;
            this.action = action;
        }
        
        // 名前で比較する
        // Separator同士は一致させない
        private class NameComparerImpl : IEqualityComparer<MenuItem>
        {
            public bool Equals(MenuItem x, MenuItem y)
            {
                if (x == null && y == null) return true;
                if (x == Separator || y == Separator) return false;
                return x?.name == y?.name;
            }

            public int GetHashCode(MenuItem obj)
            {
                return obj.name?.GetHashCode() ?? 0;
            }
        }
    }

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