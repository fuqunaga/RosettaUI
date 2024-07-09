using System;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression, in SliderOption? option = null) 
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, null, option);

        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression, TMinMax range, in SliderOption? option = null) 
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, range, option);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Expression<Func<TMinMax>> targetExpression, in SliderOption? option = null) 
            => MinMaxSlider(label, targetExpression, null, option);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Expression<Func<TMinMax>> targetExpression, TMinMax range, in SliderOption? option = null) 
            => MinMaxSlider(label, targetExpression, ConstGetter.Create(range), option);


        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression, Action<TMinMax> writeValue, in SliderOption? option = null)
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression),targetExpression.Compile(), writeValue, option);

        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression, Action<TMinMax> writeValue, TMinMax range, in SliderOption? option = null)
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, range, option);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Func<TMinMax> readValue, Action<TMinMax> writeValue, in SliderOption? option = null) 
            => MinMaxSlider(label, Binder.Create(readValue, writeValue), null, option);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Func<TMinMax> readValue, Action<TMinMax> writeValue, TMinMax range, in SliderOption? option = null) 
            => MinMaxSlider(label, Binder.Create(readValue, writeValue), ConstGetter.Create(range), option);


        public static Element MinMaxSliderReadOnly<TMinMax>(Expression<Func<TMinMax>> targetExpression, in SliderOption? option = null)
            => MinMaxSliderReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression),
                option
            );

        public static Element MinMaxSliderReadOnly<TMinMax>(Expression<Func<TMinMax>> targetExpression, TMinMax range, in SliderOption? option = null)
            => MinMaxSliderReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression),
                range,
                option
            );
        
        public static Element MinMaxSliderReadOnly<TMinMax>(LabelElement label, Func<TMinMax> readValue, in SliderOption? option = null) 
            => MinMaxSlider(label, readValue, null, option);
        
        public static Element MinMaxSliderReadOnly<TMinMax>(LabelElement label, Func<TMinMax> readValue, TMinMax range, in SliderOption? option = null) 
            => MinMaxSlider(label, Binder.Create(readValue, null), ConstGetter.Create(range), option);
        

        public static Element MinMaxSlider<TMinMax>(LabelElement label,
            Expression<Func<TMinMax>> targetExpression,
            IGetter<TMinMax> rangeGetter,
            in SliderOption? option = null
            )
        {
            var binder = UIInternalUtility.CreateBinder(targetExpression);

            return MinMaxSlider(label, binder, rangeGetter, option);
        }

        public static Element MinMaxSlider<TMinMax>(LabelElement label, IBinder<TMinMax> minMaxBinder, IGetter<TMinMax> rangeGetter, in SliderOption? option = null)
        {
            IBinder binder = minMaxBinder;
            var (minName, maxName) = TypeUtility.GetMinMaxPropertyOrFieldName(typeof(TMinMax));
            Assert.IsTrue(minName != null && maxName != null, $"{typeof(TMinMax)} does not have members for MinMaxSlider.\nRequired member names [{string.Join(",", TypeUtility.MinMaxMemberNamePairs.Select(pair => $"({pair.Item1}, {pair.Item2})"))}]");
            
            var minMaxType = minMaxBinder.ValueType;
            var isInnerType =  minMaxType.IsGenericType && minMaxType.GetGenericTypeDefinition() == typeof(MinMax<>);
            if (!isInnerType)
            {
                binder = ConvertMinMaxBinder(minMaxBinder);
            }
            
            var minGetter = PropertyOrFieldGetter.Create(rangeGetter, minName);
            var maxGetter = PropertyOrFieldGetter.Create(rangeGetter, maxName);
            
            return MinMaxSlider(label, binder, new SliderElementOption(minGetter, maxGetter, option));  
            
            IBinder ConvertMinMaxBinder(IBinder binderFrom)
            {
                var fieldType = TypeUtility.GetPropertyOrFieldType(typeof(TMinMax), minName);
                var binderType = typeof(ConvertToMinMaxBinder<,>).MakeGenericType(typeof(TMinMax), fieldType);
                
                return (IBinder)Activator.CreateInstance(binderType, binderFrom);
            }
        }
        
        public static Element MinMaxSlider(LabelElement label, IBinder binder, in SliderElementOption elementOption)
        {
            var contents = BinderToElement.CreateMinMaxSliderElement(label, binder, elementOption);
            if (contents == null) return null;

            UIInternalUtility.SetInteractableWithBinder(contents, binder);

            return contents;
        }
    }
}