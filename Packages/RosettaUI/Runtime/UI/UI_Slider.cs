using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element Slider<T>(Expression<Func<T>> targetExpression)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, T max)
            => Slider(targetExpression, default(T), max);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, T min, T max)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                min,
                max
            );


        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression)
        {
            var (rangeMinGetter, rangeMaxGetter) = CreateMinMaxGetterFromRangeAttribute(targetExpression);
            return Slider(label, CreateBinder(targetExpression), rangeMinGetter, rangeMaxGetter);
        }
        
        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T max)
        {
            return Slider(label, targetExpression, default, max);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T min, T max)
        {
            return Slider(label,
                CreateBinder(targetExpression),
                ConstGetter.Create(min),
                ConstGetter.Create(max)
                );
        }


        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> writeValue)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> writeValue, T max)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, max);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> writeValue, T min, T max)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, min, max);

        
        public static Element Slider<T>(LabelElement label, Func<T> readValue, Action<T> writeValue) 
            => Slider(label, Binder.Create(readValue, writeValue), null, null);

        public static Element Slider<T>(LabelElement label, Func<T> readValue, Action<T> writeValue, T max) 
            => Slider(label, readValue, writeValue, default, max);

        public static Element Slider<T>(LabelElement label, Func<T> readValue, Action<T> writeValue, T min, T max)
        {
            return Slider(label, 
                Binder.Create(readValue, writeValue), 
                ConstGetter.Create(min), 
                ConstGetter.Create(max));
        }

        

        public static Element SliderReadOnly<T>(Expression<Func<T>> targetExpression)
        {
            var (rangeMinGetter, rangeMaxGetter) = CreateMinMaxGetterFromRangeAttribute(targetExpression);
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                CreateReadOnlyBinder(targetExpression),
                rangeMinGetter,
                rangeMaxGetter
            );
        }

        public static Element SliderReadOnly<T>(Expression<Func<T>> targetExpression, T max)
            => SliderReadOnly(targetExpression, default, max);

        public static Element SliderReadOnly<T>(Expression<Func<T>> targetExpression, T min, T max)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                CreateReadOnlyBinder(targetExpression),
                ConstGetter.Create(min),
                ConstGetter.Create(max)
            );
        }


        public static Element SliderReadOnly<T>(LabelElement label, Func<T> readValue)
            => Slider(label, Binder.Create(readValue, null), null, null);
        
        public static Element SliderReadOnly<T>(LabelElement label, Func<T> readValue, T max)
            => SliderReadOnly(label, readValue, default, max);

        public static Element SliderReadOnly<T>(LabelElement label, Func<T> readValue, T min, T max)
            => Slider(label, readValue, null, min, max);



        public static Element Slider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
            => Slider(label, binder, new SliderOption() {minGetter = minGetter, maxGetter = maxGetter});
        
        public static Element Slider(LabelElement label, IBinder binder, SliderOption option)
        {
            var contents = BinderToElement.CreateSliderElement(label, binder, option);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }
    }
}