using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static partial class UI
    {
        #region Button

        public static ButtonElement Button(LabelElement label, Action onClick = null)
        {
            return new ButtonElement(label?.getter, onClick);
        }

        public static ButtonElement Button(LabelElement label, Action<ButtonElement> onClick)
        {
            var button = Button(label);
            button.onClick += () => onClick(button);

            return button;
        }

        #endregion

        
        #region PopupMenuButton
        
        public static ButtonElement PopupMenuButton(LabelElement label, Func<IEnumerable<MenuItem>> createMenuItems)
        {
            return Button(label, (buttonElement) =>
            {
                PopupMenu.Show(
                    createMenuItems?.Invoke(),
                    InputUtility.GetMousePositionUICoordinate(),
                    buttonElement);
            });
        }
        
        #endregion
    }
}