using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
                
        #region Space

        public static SpaceElement Space() => new SpaceElement();
        
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

        public static DropdownElement Dropdown(Expression<Func<int>> targetExpression, IEnumerable<string> options,
            Action<int> onValueChanged = null)
        {
            return Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options,
                onValueChanged);
        }

        public static DropdownElement Dropdown(LabelElement label, Expression<Func<int>> targetExpression,
            IEnumerable<string> options, Action<int> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            return Dropdown(label, binder, options, onValueChanged);
        }

        public static DropdownElement Dropdown(LabelElement label, IBinder<int> binder, IEnumerable<string> options, Action<int> onValueChanged = null)
        {
            var element = new DropdownElement(label, binder, options);
            SetInteractableWithBinder(element, binder);
            return element;
        }

        public static DropdownElement DropdownReadOnly(LabelElement label, Func<int> getValue,
            IEnumerable<string> options, Action<int> onValueChanged = null)
        {
            return Dropdown(label, Binder.Create(getValue, onValueChanged), options, onValueChanged);
        }

        #endregion

        
        #region PopupMenu

        public static PopupMenuElement Popup(Element childElement, Func<IEnumerable<MenuItem>> createMenuItems)
        {
            return new PopupMenuElement(childElement, createMenuItems);
        }
        
        
        #endregion



    }
}