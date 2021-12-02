using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression) 
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, null, null);

        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T max) 
            => MinMaxSlider(targetExpression, default, max);


        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T min, T max) 
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression,
                ConstGetter.Create(min), ConstGetter.Create(max));


        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression) 
            => MinMaxSlider(label, targetExpression, null, null);

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T max) 
            => MinMaxSlider(label, targetExpression, default, max);

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T min, T max) 
            => MinMaxSlider(label, targetExpression, ConstGetter.Create(min), ConstGetter.Create(max));
        

        public static Element MinMaxSlider<T>(LabelElement label, Func<MinMax<T>> readValue, Action<MinMax<T>> writeValue) 
            => MinMaxSlider(label, Binder.Create(readValue, writeValue), null, null);

        public static Element MinMaxSlider<T>(LabelElement label, Func<MinMax<T>> readValue,  Action<MinMax<T>> writeValue, T max) 
            => MinMaxSlider(label, readValue, writeValue, default, max);

        public static Element MinMaxSlider<T>(LabelElement label, Func<MinMax<T>> readValue, Action<MinMax<T>> writeValue, T min, T max) 
            => MinMaxSlider(label, Binder.Create(readValue, writeValue), ConstGetter.Create(min), ConstGetter.Create(max));



        public static Element MinMaxSliderReadOnly<T>(Expression<Func<MinMax<T>>> targetExpression)
            => MinMaxSliderReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression), 
                ExpressionUtility.CreateReadFunc(targetExpression)
                );

        public static Element MinMaxSliderReadOnly<T>(Expression<Func<MinMax<T>>> targetExpression, T max) 
            => MinMaxSliderReadOnly(targetExpression, default, max);


        public static Element MinMaxSliderReadOnly<T>(Expression<Func<MinMax<T>>> targetExpression, T min, T max)
            => MinMaxSliderReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression), 
                min, max);
        
        public static Element MinMaxSliderReadOnly<T>(LabelElement label, Func<MinMax<T>> readValue) 
            => MinMaxSlider(label, readValue, null);

        public static Element MinMaxSliderReadOnly<T>(LabelElement label, Func<MinMax<T>> readValue, T max) 
            => MinMaxSlider(label, readValue, default, max);

        public static Element MinMaxSliderReadOnly<T>(LabelElement label, Func<MinMax<T>> readValue, T min, T max) 
            => MinMaxSlider(label, Binder.Create(readValue, null), ConstGetter.Create(min), ConstGetter.Create(max));
        

        public static Element MinMaxSlider<T>(LabelElement label,
            Expression<Func<MinMax<T>>> targetExpression,
            IGetter<T> minGetter,
            IGetter<T> maxGetter
            )
        {
            var binder = CreateBinder(targetExpression);
            return MinMaxSlider(label, binder, minGetter, maxGetter);
        }
        
        public static Element MinMaxSlider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
            => MinMaxSlider(label, binder, new SliderOption() {minGetter = minGetter, maxGetter = maxGetter});

        public static Element MinMaxSlider(LabelElement label, IBinder binder, SliderOption option)
        {
            var contents = BinderToElement.CreateMinMaxSliderElement(label, binder, option);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }
    }
}