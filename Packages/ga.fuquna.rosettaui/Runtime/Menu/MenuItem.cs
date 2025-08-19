using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public interface IMenuItem
    {
        string Name { get; }
        
        public static IEqualityComparer<IMenuItem> NameComparer { get; } = new NameComparerImpl();
        
                
        // 名前で比較する
        // Separator同士は一致させない
        private class NameComparerImpl : IEqualityComparer<IMenuItem>
        {
            public bool Equals(IMenuItem x, IMenuItem y)
            {
                if (x == null && y == null) return true;
                if (x is MenuItemSeparator || y is MenuItemSeparator) return false;
                return x?.Name == y?.Name;
            }

            public int GetHashCode(IMenuItem obj)
            {
                return obj.Name?.GetHashCode() ?? 0;
            }
        }
    }
    
    public class MenuItemSeparator : IMenuItem
    {
        public string Name { get; set; }
        public MenuItemSeparator(string name) => Name = name;
    }


    public class MenuItem : IMenuItem
    {
        [Obsolete("Use new MenuItemSeparator() instead.")]
        public static readonly IMenuItem Separator = new MenuItemSeparator("");
        
        public string name;
        public bool isChecked;
        public bool isEnable = true;
        public Action action;
        
        public string Name => name;

        public MenuItem()
        {
        }

        public MenuItem(string name, Action action = null)
        {
            this.name = name;
            this.action = action;
            if (action == null)
            {
                isEnable = false;
            }
        }
    }
}