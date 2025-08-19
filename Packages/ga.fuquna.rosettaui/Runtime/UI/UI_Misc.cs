using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI
{
    public static partial class UI
    {
                
        #region Space

        public static SpaceElement Space() => new();
        
        #endregion
        
        #region Image

        public static ImageElement Image(Texture texture)
            => Image(ConstGetter.Create(texture));
        
        public static ImageElement Image(Func<Texture> readValue)
            => Image(Getter.Create(readValue));

        public static ImageElement Image(IGetter<Texture> getter)
            => new ImageElement(getter);
        
        #endregion
        
        #region PopupMenu

        public static PopupMenuElement Popup(Element childElement, Func<IEnumerable<IMenuItem>> createMenuItems, MouseButton mouseButton = MouseButton.RightMouse)
        {
            return new PopupMenuElement(childElement, createMenuItems, mouseButton);
        }
        
        #endregion

        #region HelpBox

        public static HelpBoxElement HelpBox(LabelElement message, HelpBoxType helpBoxType = HelpBoxType.None) 
            => new(message, helpBoxType);

        #endregion
        
        #region Clickable

        public static ClickableElement Clickable(Element childElement, Action<IClickEvent> onClick)
            => new ClickableElement(childElement, onClick);

        #endregion
    }
}