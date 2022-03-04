using System;
using System.Collections.Generic;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine.UIElements;

namespace UnityInternalAccess.Custom
{
    public class PopupFieldCustomMenu<T> : PopupField<T>
    {
        public event Action<GenericDropdownMenu> onMenuCreated;

        public PopupFieldCustomMenu(List<T> choices, int defaultIndex) : base(choices, defaultIndex)
        {
        }

        internal override void AddMenuItems(IGenericMenu menu)
        {
            base.AddMenuItems(menu);

            if (menu is GenericDropdownMenu genericDropdownMenu)
            {
                onMenuCreated?.Invoke(genericDropdownMenu);
            }
        }
    }
}