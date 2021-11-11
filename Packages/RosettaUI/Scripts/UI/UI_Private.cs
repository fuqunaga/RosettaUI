using System;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static partial class UI
    {
        static IBinder<T> CreateBinder<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged)
        {
            var binder = ExpressionUtility.CreateBinder(targetExpression);
            if (binder != null)
            {
                binder.onValueChanged += onValueChanged;
            }

            return binder;
        }


        static (IGetter<T>, IGetter<T>) CreateMinMaxGetterFromRangeAttribute<T>(Expression<Func<T>> targetExpression)
        {
            var rangeAttribute = typeof(IConvertible).IsAssignableFrom(typeof(T))
                ? ExpressionUtility.GetAttribute<T, RangeAttribute>(targetExpression)
                : null;

            return RangeUtility.CreateGetterMinMax<T>(rangeAttribute);
        }


        static void SetInteractableWithBinder(Element element, IBinder binder)
        {
            element.Interactable = !binder.IsReadOnly;
        }
    }
}