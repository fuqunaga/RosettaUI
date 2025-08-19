#nullable  enable

using System;
using System.Collections.Generic;
using RosettaUI.UIToolkit.NestedDropdownMenuSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// DropdownMenu that supports hierarchical submenus based on GenericDropdownMenu.
    /// </summary>
    public class NestedDropdownMenu
    {
        private readonly Dictionary<string, SingleMenu> _menuTable = new();

        private SingleMenu RootMenu
        {
            get
            {
                if (!_menuTable.TryGetValue(string.Empty, out var menu))
                {
                    menu = new SingleMenu();
                    _menuTable[string.Empty] = menu;

                }

                return menu;
            }
        }
        
        public IEnumerable<SingleMenu> SingleMenus => _menuTable.Values;


        #region IGenericMenu like methods

        public void AddItem(string? itemName, bool isChecked, Action? action)
        {
            var (menu, label) = GetMenuAndLabel(itemName);
            menu.AddItem(label, isChecked, action);
        }

        public void AddItem(string? itemName, bool isChecked, Action<object?>? action, object? data)
        {
            var (menu, label) = GetMenuAndLabel(itemName);
            menu.AddItem(label, isChecked, () => action?.Invoke(data));
        }

        public void AddDisabledItem(string? itemName, bool isChecked)
        {
            var (menu, label) = GetMenuAndLabel(itemName);
            menu.AddDisabledItem(label, isChecked);
        }

        public void AddSeparator(string? path)
        {
            var (menu, label) = GetMenuAndLabel(path);
            menu.AddSeparator(label);
        }

        public void DropDown(Rect position, VisualElement? targetElement = null, bool anchored = false)
        {
            RootMenu.DropDown(position, targetElement, anchored);
        }

        #endregion


        private (SingleMenu menu, string label) GetMenuAndLabel(string? itemName)
        {
            var (path, label) = ParseItemNameToPathAndLabel(itemName ?? string.Empty);

            if (!_menuTable.TryGetValue(path, out var menu))
            {
                menu = new SingleMenu();
                _menuTable[path] = menu;

                var isSubMenu = !string.IsNullOrEmpty(path);
                if (isSubMenu)
                {
                    // 親メニューにサブメニューアイテムを追加
                    var (parentMenu, menuLabel) = GetMenuAndLabel(path);
                    
                    const int delayMs = 500;
                    parentMenu.AddSubMenuItem(menuLabel, delayMs, menu);
                }
            }

            return (menu, label);
        }
        

        public static (string path, string label) ParseItemNameToPathAndLabel(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
            {
                return (string.Empty, string.Empty);
            }

            var lastSlashIndex = itemName.LastIndexOf('/');
            if (lastSlashIndex == -1)
            {
                return (string.Empty, itemName);
            }

            var path = itemName[..lastSlashIndex];
            var name = itemName[(lastSlashIndex + 1)..];
            return (path, name);
        }
    }
}