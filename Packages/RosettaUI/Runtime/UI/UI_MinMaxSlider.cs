using System;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression) 
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, null);

        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression, TMinMax range) 
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, range);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Expression<Func<TMinMax>> targetExpression) 
            => MinMaxSlider(label, targetExpression, null);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Expression<Func<TMinMax>> targetExpression, TMinMax range) 
            => MinMaxSlider(label, targetExpression, ConstGetter.Create(range));


        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression, Action<TMinMax> writeValue)
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression),targetExpression.Compile(), writeValue);

        public static Element MinMaxSlider<TMinMax>(Expression<Func<TMinMax>> targetExpression, Action<TMinMax> writeValue, TMinMax range)
            => MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, range);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Func<TMinMax> readValue, Action<TMinMax> writeValue) 
            => MinMaxSlider(label, Binder.Create(readValue, writeValue), null);
        
        public static Element MinMaxSlider<TMinMax>(LabelElement label, Func<TMinMax> readValue, Action<TMinMax> writeValue, TMinMax range) 
            => MinMaxSlider(label, Binder.Create(readValue, writeValue), ConstGetter.Create(range));


        public static Element MinMaxSliderReadOnly<TMinMax>(Expression<Func<TMinMax>> targetExpression)
            => MinMaxSliderReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression), 
                ExpressionUtility.CreateReadFunc(targetExpression)
                );

        public static Element MinMaxSliderReadOnly<TMinMax>(Expression<Func<TMinMax>> targetExpression, TMinMax range)
            => MinMaxSliderReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression), 
                range);
        
        public static Element MinMaxSliderReadOnly<TMinMax>(LabelElement label, Func<TMinMax> readValue) 
            => MinMaxSlider(label, readValue, null);
        
        public static Element MinMaxSliderReadOnly<TMinMax>(LabelElement label, Func<TMinMax> readValue, TMinMax range) 
            => MinMaxSlider(label, Binder.Create(readValue, null), ConstGetter.Create(range));
        

        public static Element MinMaxSlider<TMinMax>(LabelElement label,
            Expression<Func<TMinMax>> targetExpression,
            IGetter<TMinMax> rangeGetter
            )
        {
            var binder = UIInternalUtility.CreateBinder(targetExpression);

            return MinMaxSlider(label, binder, rangeGetter);
        }

        public static Element MinMaxSlider<TMinMax>(LabelElement label, IBinder<TMinMax> minMaxBinder, IGetter<TMinMax> rangeGetter)
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
            
            return MinMaxSlider(label, binder, new SliderElementOption(minGetter, maxGetter));  
            
            IBinder ConvertMinMaxBinder(IBinder binderFrom)
            {
                var fieldType = TypeUtility.GetPropertyOrFieldType(typeof(TMinMax), minName);
                var binderType = typeof(ConvertToMinMaxBinder<,>).MakeGenericType(typeof(TMinMax), fieldType);
                
                return (IBinder)Activator.CreateInstance(binderType, binderFrom);
            }
        }
        
        public static Element MinMaxSlider(LabelElement label, IBinder binder, in SliderElementOption? elementOption)
        {
            var contents = BinderToElement.CreateMinMaxSliderElement(label, binder, elementOption ?? SliderElementOption.Default);
            if (contents == null) return null;

            UIInternalUtility.SetInteractableWithBinder(contents, binder);

            return contents;
        }
    }
}