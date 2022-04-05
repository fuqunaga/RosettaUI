using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element Field<T>(Expression<Func<T>> targetExpression)
        {
            return Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression);
        }

        public static Element Field<T>(LabelElement label, Expression<Func<T>> targetExpression)
        {
            var binder = CreateBinder(targetExpression);
            return Field(label, binder);
        }

        public static Element Field<T>(Expression<Func<T>> targetExpression, Action<T> writeValue)
            => Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue);
        
        public static Element Field<T>(LabelElement label, Func<T> readValue, Action<T> writeValue)
            => Field(label, Binder.Create(readValue, writeValue));

        public static Element Field(LabelElement label, IBinder binder)
        {
            var element = BinderToElement.CreateFieldElement(label, binder);
            if (element != null) SetInteractableWithBinder(element, binder);

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