using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class PopupMenuManipulator : Clickable
    {
        private readonly Func<IEnumerable<MenuItem>> _createMenuItemsFunc;

        public MouseButton Button
        {
            set
            {
                activators.Clear();
                activators.Add(new ManipulatorActivationFilter {button = value});
            }
        }

        public PopupMenuManipulator(Func<IEnumerable<MenuItem>> createMenuItemsFunc) : base((Action)null)
        {
            clickedWithEventInfo += OnClick;
            _createMenuItemsFunc = createMenuItemsFunc;
            Button = MouseButton.RightMouse;
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
            var menuItems = evt.MenuItems;
            var newItems = _createMenuItemsFunc();
            
            menuItems.InsertRange(0, menuItems.Any() 
                ? newItems.Append(MenuItem.Separator)
                : newItems); 
        }


        private void OnPopupMenuBubbleUp(PopupMenuEvent evt)
        {
            if (evt.target != target) return;
            
            PopupMenuUtility.Show(
                evt.MenuItems,
                evt.Position,
                target
            );
            
            evt.StopImmediatePropagation();
        }
    }
}