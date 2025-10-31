using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element Toggle(Expression<Func<bool>> targetExpression)
        {
            return Toggle(ExpressionUtility.CreateLabelString(targetExpression), targetExpression);
        }

        public static Element Toggle(LabelElement label, Expression<Func<bool>> targetExpression)
        {
            var binder = UIInternalUtility.CreateBinder(targetExpression);
            return Toggle(label, binder);
        }

        public static Element Toggle(Expression<Func<bool>> targetExpression, Action<bool> writeValue)
            => Toggle(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue);
        
        public static Element Toggle(LabelElement label, Func<bool> readValue, Action<bool> writeValue)
            => Toggle(label, Binder.Create(readValue, writeValue));

        public static Element Toggle(LabelElement label, IBinder binder)
        {
            if (binder is not IBinder<bool> boolBinder) return null;
            
            var element = new ToggleElement(label, boolBinder)
            {
                isLabelRight = true
            };
            UIInternalUtility.SetInteractableWithBinder(element, binder);

            return element;
        }

        public static Element ToggleReadOnly(Expression<Func<bool>> targetExpression)
            => ToggleReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression
                ));

        public static Element ToggleReadOnly(LabelElement label, Func<bool> readValue)
            => Toggle(label, readValue, null);
    }
}