using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element Field<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, onValueChanged);
        }

        public static Element Field<T>(LabelElement label, Expression<Func<T>> targetExpression,
            Action<T> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            return Field(label, binder);
        }

        public static Element Field(LabelElement label, IBinder binder)
        {
            var element = BinderToElement.CreateFieldElement(label, binder);
            if (element != null) SetInteractableWithBinder(element, binder);

            return element;
        }

        public static Element FieldReadOnly<T>(LabelElement label, Func<T> getValue)
            => Field(label, Binder.Create(getValue, null));
    }
}