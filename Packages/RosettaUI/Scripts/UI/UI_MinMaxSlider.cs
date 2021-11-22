using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(targetExpression, default, max, onValueChanged);
        }


        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T min, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression,
                ConstGetter.Create(min), ConstGetter.Create(max), onValueChanged);
        }

        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, null, null,
                onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, default, max, onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T min,
            T max, Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, ConstGetter.Create(min), ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, null, null, onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label,
            Expression<Func<MinMax<T>>> targetExpression,
            IGetter<T> minGetter,
            IGetter<T> maxGetter,
            Action<MinMax<T>> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression);
            return MinMaxSlider(label, binder, minGetter, maxGetter);
        }

        public static Element MinMaxSlider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
        {
            var contents = BinderToElement.CreateMinMaxSliderElement(label, binder, minGetter, maxGetter);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }
    }
}