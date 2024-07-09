using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element Slider<T>(Expression<Func<T>> targetExpression, in SliderOption? option = null)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, option);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, T max, in SliderOption? option = null)
            => Slider(targetExpression, default(T), max, option);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, T min, T max, in SliderOption? option = null)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                min,
                max,
                option
            );


        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, in SliderOption? option = null)
        {
            var (rangeMinGetter, rangeMaxGetter) = UIInternalUtility.CreateMinMaxGetterFromRangeAttribute(targetExpression);
            return Slider(label, UIInternalUtility.CreateBinder(targetExpression), rangeMinGetter, rangeMaxGetter, option);
        }
        
        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T max, in SliderOption? option = null)
        {
            return Slider(label, targetExpression, default, max, option);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T min, T max, in SliderOption? option = null)
        {
            return Slider(label,
                UIInternalUtility.CreateBinder(targetExpression),
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                option
            );
        }


        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> writeValue, in SliderOption? option = null)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, option);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> writeValue, T max, in SliderOption? option = null)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, max, option);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> writeValue, T min, T max, in SliderOption? option = null)
            => Slider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, min, max, option);

        
        public static Element Slider<T>(LabelElement label, Func<T> readValue, Action<T> writeValue, in SliderOption? option = null) 
            => Slider(label, Binder.Create(readValue, writeValue), null, null, option);

        public static Element Slider<T>(LabelElement label, Func<T> readValue, Action<T> writeValue, T max, in SliderOption? option = null) 
            => Slider(label, readValue, writeValue, default, max, option);

        public static Element Slider<T>(LabelElement label, Func<T> readValue, Action<T> writeValue, T min, T max, in SliderOption? option = null)
        {
            return Slider(label, 
                Binder.Create(readValue, writeValue), 
                ConstGetter.Create(min), 
                ConstGetter.Create(max),
                option
            );
        }

        

        public static Element SliderReadOnly<T>(Expression<Func<T>> targetExpression, in SliderOption? option = null)
        {
            var (rangeMinGetter, rangeMaxGetter) = UIInternalUtility.CreateMinMaxGetterFromRangeAttribute(targetExpression);
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                UIInternalUtility.CreateReadOnlyBinder(targetExpression),
                rangeMinGetter,
                rangeMaxGetter,
                option
            );
        }

        public static Element SliderReadOnly<T>(Expression<Func<T>> targetExpression, T max, in SliderOption? option = null)
            => SliderReadOnly(targetExpression, default, max, option);

        public static Element SliderReadOnly<T>(Expression<Func<T>> targetExpression, T min, T max, in SliderOption? option = null)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                UIInternalUtility.CreateReadOnlyBinder(targetExpression),
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                option
            );
        }


        public static Element SliderReadOnly<T>(LabelElement label, Func<T> readValue, in SliderOption? option = null)
            => Slider(label, Binder.Create(readValue, null), null, null, option);
        
        public static Element SliderReadOnly<T>(LabelElement label, Func<T> readValue, T max, in SliderOption? option = null)
            => SliderReadOnly(label, readValue, default, max, option);

        public static Element SliderReadOnly<T>(LabelElement label, Func<T> readValue, T min, T max, in SliderOption? option = null)
            => Slider(label, readValue, null, min, max, option);



        public static Element Slider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter, in SliderOption? option = null)
            => Slider(label, binder, new SliderElementOption(minGetter, maxGetter, option));
        
        public static Element Slider(LabelElement label, IBinder binder, in SliderElementOption elementOption)
        {
            var contents = BinderToElement.CreateSliderElement(label, binder, elementOption);
            if (contents == null) return null;

            UIInternalUtility.SetInteractableWithBinder(contents, binder);

            return contents;
        }
    }
}