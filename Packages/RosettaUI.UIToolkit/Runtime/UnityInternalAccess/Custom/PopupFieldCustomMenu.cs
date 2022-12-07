using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class PopupFieldCustomMenu<T> : PopupField<T>
    {
        public event Action<GenericDropdownMenu> onMenuCreated;

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