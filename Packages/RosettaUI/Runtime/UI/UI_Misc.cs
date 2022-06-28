using System;
using System.Collections.Generic;
using UnityEngine;

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
        
        #region PopupMenu

        public static PopupMenuElement Popup(Element childElement, Func<IEnumerable<MenuItem>> createMenuItems)
        {
            return new PopupMenuElement(childElement, createMenuItems);
        }
        
        
        #endregion

        #region HelpBox

        public static HelpBoxElement HelpBox(LabelElement message, HelpBoxType helpBoxType = HelpBoxType.None) 
            => new(message, helpBoxType);

        #endregion
    }
}