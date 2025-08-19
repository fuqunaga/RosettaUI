﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class PopupMenuManipulator : Clickable
    {
        private readonly Func<IEnumerable<IMenuItem>> _createMenuItemsFunc;

        public MouseButton Button
        {
            set
            {
                activators.Clear();
                activators.Add(new ManipulatorActivationFilter {button = value});
            }
        }

        public PopupMenuManipulator(Func<IEnumerable<IMenuItem>> createMenuItemsFunc, MouseButton mouseButton = MouseButton.RightMouse) : base((Action)null)
        {
            clickedWithEventInfo += OnClick;
            _createMenuItemsFunc = createMenuItemsFunc;
            Button = mouseButton;
        }

        private void OnClick(EventBase evt)
        {
            var success = evt.TryGetPosition(out var position);
            if (!success) return;

            if (evt.target is VisualElement visualElement)
            {
                PopupMenuEvent.Send(visualElement, position);
            }
        }

        
        protected override void RegisterCallbacksOnTarget()
        {
            base.RegisterCallbacksOnTarget();

            target.RegisterCallback<PopupMenuEvent>(OnPopupMenuTrickleDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PopupMenuEvent>(OnPopupMenuBubbleUp);
        }


        protected override void UnregisterCallbacksFromTarget()
        {
            base.UnregisterCallbacksFromTarget();

            target.UnregisterCallback<PopupMenuEvent>(OnPopupMenuTrickleDown);
            target.UnregisterCallback<PopupMenuEvent>(OnPopupMenuBubbleUp);
        }


        private void OnPopupMenuTrickleDown(PopupMenuEvent evt)
        {
            if (_createMenuItemsFunc == null) return;
            evt.MenuItemSets.Add(_createMenuItemsFunc());
        }


        private void OnPopupMenuBubbleUp(PopupMenuEvent evt)
        {
            if (evt.target != target) return;
            
            // メニューアイテムを整理する
            // 後から追加されたMenuItemSetを優先してリストの先頭に追加する
            // 同じ名前のメニューアイテムはPopupMenuUtility内のGenericDropdownMenuで先に登録されたもののみ適用されるが、
            // セパレーターがメニューの最初か最後にあるのは避けたいので、ここで重複チェックしセパレーターの必要性を判断する
            using var _0 = ListPool<IMenuItem>.Get(out var menuItems);
            using var _1 = ListPool<IMenuItem>.Get(out var addItems);
            using var _2 = HashSetPool<string>.Get(out var existingPaths);

            foreach (var addItemSet in Enumerable.Reverse(evt.MenuItemSets))
            {
                addItems.Clear();
                addItems.AddRange(addItemSet.Except(menuItems, IMenuItem.NameComparer));
                if ( addItems.Count <= 0 ) continue;

                var addItemPaths = addItems
                    .Select(item => NestedDropdownMenu.ParseItemNameToPathAndLabel(item.Name).path)
                    .Distinct();
                
                foreach(var path in addItemPaths)
                {
                    if (!existingPaths.Add(path))
                    {
                        // 既に存在するパスはセパレーターを追加する
                        menuItems.Add(new MenuItemSeparator($"{path}/"));
                    }
                }

                menuItems.AddRange(addItems);
            }
            
            PopupMenuUtility.Show(
                menuItems,
                evt.Position,
                target
            );
            
            evt.StopImmediatePropagation();
        }
    }
}