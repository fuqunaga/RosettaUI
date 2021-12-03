using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static partial class UI
    {
                
        #region Space

        public static SpaceElement Space() => new SpaceElement();
        
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

        
        #region Dropdown

        public static DropdownElement Dropdown(Expression<Func<int>> targetExpression, IEnumerable<string> options) 
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options);

        public static DropdownElement Dropdown(LabelElement label, Expression<Func<int>> targetExpression, IEnumerable<string> options) 
            => Dropdown(label, CreateBinder(targetExpression), options);

        public static DropdownElement Dropdown(LabelElement label, IBinder<int> binder, IEnumerable<string> options)
        {
            var element = new DropdownElement(label, binder, options);
            SetInteractableWithBinder(element, binder);
            return element;
        }

        public static DropdownElement Dropdown(LabelElement label, Func<int> readValue, Action<int> writeValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, writeValue), options);

        
        public static DropdownElement DropdownReadOnly(Expression<Func<int>> targetExpression, IEnumerable<string> options)
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), CreateReadOnlyBinder(targetExpression),
                options);

        public static DropdownElement DropdownReadOnly(LabelElement label, Func<int> readValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, null), options);

        #endregion

        
        #region PopupMenu

        public static PopupMenuElement Popup(Element childElement, Func<IEnumerable<MenuItem>> createMenuItems)
        {
            return new PopupMenuElement(childElement, createMenuItems);
        }
        
        
        #endregion



    }
}