using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element Slider<T>(Expression<Func<T>> targetExpression, T max, Action<T> onValueChanged = null)
        {
            return Slider(targetExpression, default, max, onValueChanged);
        }


        public static Element Slider<T>(Expression<Func<T>> targetExpression, T min, T max, Action<T> onValueChanged = null)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                null,
                null,
                onValueChanged);
        }


        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T max, Action<T> onValueChanged = null)
        {
            return Slider(label, targetExpression, default, max, onValueChanged);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T min, T max, Action<T> onValueChanged = null)
        {
            return Slider(label,
                targetExpression,
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Slider(label, targetExpression, null, null, onValueChanged);
        }

        public static Element Slider<T>(LabelElement label,
            Expression<Func<T>> targetExpression,
            IGetter minGetter,
            IGetter maxGetter,
            Action<T> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            if (minGetter == null || maxGetter == null)
            {
                var (rangeMinGetter, rangeMaxGetter) = CreateMinMaxGetterFromRangeAttribute(targetExpression);
                minGetter ??= rangeMinGetter;
                maxGetter ??= rangeMaxGetter;
            }

            return Slider(label, binder, minGetter, maxGetter);
        }

        public static Element Slider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
        {
            var contents = BinderToElement.CreateSliderElement(label, binder, minGetter, maxGetter);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }
    }
}