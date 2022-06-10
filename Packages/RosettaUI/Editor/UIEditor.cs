using System;
using System.Linq.Expressions;
using Object = UnityEngine.Object;

namespace RosettaUI.Editor
{
    public static class UIEditor
    {
        public static Element ObjectField<T>(Expression<Func<T>> targetExpression) 
            where T : Object
        {
            return ObjectField(ExpressionUtility.CreateLabelString(targetExpression), targetExpression);
        }

        public static Element ObjectField<T>(LabelElement label, Expression<Func<T>> targetExpression)
            where T : Object
        {
            var binder = UIInternalUtility.CreateBinder(targetExpression);
            return ObjectField(label, binder);
        }

        public static Element ObjectField<T>(Expression<Func<T>> targetExpression, Action<T> writeValue)
            where T : Object
            => ObjectField(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue);
        
        public static Element ObjectField<T>(LabelElement label, Func<T> readValue, Action<T> writeValue)
            where T : Object
            => ObjectField(label, Binder.Create(readValue, writeValue));

        public static Element ObjectField<T>(LabelElement label, IBinder<T> binder)
            where T : Object
        {
            var objectBinder = new CastBinder<T, Object>(binder);
            
            var element = new ObjectFieldElement(label, objectBinder, typeof(T));
            UIInternalUtility.SetInteractableWithBinder(element, binder);

            return element;
        }

        public static Element ObjectFieldReadOnly<T>(Expression<Func<T>> targetExpression)
            where T : Object
            => ObjectFieldReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression
                ));

        public static Element ObjectFieldReadOnly<T>(LabelElement label, Func<T> readValue)
            where T : Object
            => ObjectField(label, readValue, null);
    }
}