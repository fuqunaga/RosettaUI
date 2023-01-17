using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element Field<T>(Expression<Func<T>> targetExpression, FieldOption option = null)
        {
            return Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, option);
        }

        public static Element Field<T>(LabelElement label, Expression<Func<T>> targetExpression, FieldOption option = null)
        {
            var binder = UIInternalUtility.CreateBinder(targetExpression);
            return Field(label, binder, option);
        }

        public static Element Field<T>(Expression<Func<T>> targetExpression, Action<T> writeValue, FieldOption option = null)
            => Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, option);
        
        public static Element Field<T>(LabelElement label, Func<T> readValue, Action<T> writeValue, FieldOption option = null)
            => Field(label, Binder.Create(readValue, writeValue), option);

        public static Element Field(LabelElement label, IBinder binder, FieldOption option = null)
        {
            var element = BinderToElement.CreateFieldElement(label, binder, option);
            if (element != null) UIInternalUtility.SetInteractableWithBinder(element, binder);

            return element;
        }

        public static Element FieldReadOnly<T>(Expression<Func<T>> targetExpression)
            => FieldReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression
                ));

        public static Element FieldReadOnly<T>(LabelElement label, Func<T> readValue)
            => Field(label, readValue, null);
    }
}