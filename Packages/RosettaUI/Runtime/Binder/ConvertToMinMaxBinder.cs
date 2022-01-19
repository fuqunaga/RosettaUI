using System;
using System.Linq.Expressions;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public class ConvertToMinMaxBinder<TMinMax, TValue> : ChildBinder<TMinMax, MinMax<TValue>>
    {
        public ConvertToMinMaxBinder(IBinder<TMinMax> binder) : base(binder, ToMinMaxFunc, ToTMinMaxFunc)
        {
        }

        public override bool IsConst => parentBinder.IsConst;

        
        #region static

        private static readonly Func<TMinMax, MinMax<TValue>> ToMinMaxFunc;
        private static readonly Func<TMinMax, MinMax<TValue>, TMinMax> ToTMinMaxFunc;

        static ConvertToMinMaxBinder()
        {
            var typeFrom = typeof(TMinMax);
            var typeTo = typeof(MinMax<TValue>);

            var (minNameFrom, maxNameFrom) = TypeUtility.GetMinMaxPropertyOrFieldName(typeof(TMinMax));
            
            var minNameTo = nameof(MinMax<TValue>.min);
            var maxNameTo = nameof(MinMax<TValue>.max);

            var pMinMaxFrom = Expression.Parameter(typeFrom);
            var pMinFrom = Expression.PropertyOrField(pMinMaxFrom, minNameFrom);
            var pMaxFrom = Expression.PropertyOrField(pMinMaxFrom, maxNameFrom);
            
            Assert.AreEqual(pMinFrom.Type, typeof(TValue), $"Type[{pMinFrom.Type}] of {typeof(TMinMax)}.min is not match TValue[{typeof(TValue)}]");
            Assert.AreEqual(pMaxFrom.Type, typeof(TValue), $"Type[{pMaxFrom.Type}] of {typeof(TMinMax)}.max is not match TValue[{typeof(TValue)}]");
            
            // (minMaxFrom) => {
            //      var minMaxTo = new MinMax<TValue>(); 
            //      minMaxTo.min = minMaxFrom.min;
            //      minMaxTo.max = minMaxFrom.max;
            //      return minMaxTo;
            // }
            var vMinMaxTo = Expression.Variable(typeTo);
            var blockTo = Expression.Block(
                new[] {vMinMaxTo},
                Expression.Assign(vMinMaxTo, Expression.New(typeTo)),
                Expression.Assign(Expression.PropertyOrField(vMinMaxTo, minNameTo), pMinFrom),
                Expression.Assign(Expression.PropertyOrField(vMinMaxTo, maxNameTo), pMaxFrom),
                vMinMaxTo
            );

            ToMinMaxFunc = Expression.Lambda<Func<TMinMax, MinMax<TValue>>>(blockTo, pMinMaxFrom).Compile();

            
            // (minMaxFrom, minMaxTo) => {
            //      minMaxFrom.min = minMaxTo.min;
            //      minMaxFrom.max = minMaxTo.max;
            //      return minMaxFrom;
            // }
            var pMinMaxTo = Expression.Parameter(typeTo);
            var blockFrom = Expression.Block(
                Expression.Assign(pMinFrom, Expression.PropertyOrField(pMinMaxTo, minNameTo)),
                Expression.Assign(pMaxFrom, Expression.PropertyOrField(pMinMaxTo, maxNameTo)),
                pMinMaxFrom
            );

            ToTMinMaxFunc = Expression.Lambda<Func<TMinMax, MinMax<TValue>, TMinMax>>(blockFrom, pMinMaxFrom, pMinMaxTo)
                .Compile();
        }

        #endregion
    }
}