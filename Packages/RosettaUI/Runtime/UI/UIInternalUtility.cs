using System;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static class UIInternalUtility
    {
        public static IBinder<T> CreateReadOnlyBinder<T>(Expression<Func<T>> targetExpression)
        {
            return Binder.Create(
                ExpressionUtility.CreateReadFunc(targetExpression),
                null
            );
        }
        
        public static IBinder<T> CreateBinder<T>(Expression<Func<T>> targetExpression)
        {
            return ExpressionUtility.CreateBinder(targetExpression);
        }


        public static (IGetter<T>, IGetter<T>) CreateMinMaxGetterFromRangeAttribute<T>(Expression<Func<T>> targetExpression)
        {
            var rangeAttribute = typeof(IConvertible).IsAssignableFrom(typeof(T))
                ? ExpressionUtility.GetAttribute<T, RangeAttribute>(targetExpression)
                : null;

            return RangeUtility.CreateGetterMinMax<T>(rangeAttribute);
        }


        public static void SetInteractableWithBinder(Element element, IBinder binder)
        {
            element.Interactable = !binder.IsReadOnly;
        }
    }
}