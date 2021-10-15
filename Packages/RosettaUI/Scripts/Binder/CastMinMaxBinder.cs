using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public class CastMinMaxBinder<TFrom, TTo> : ChildBinder<MinMax<TFrom>, MinMax<TTo>>
    {
        public CastMinMaxBinder(IBinder<MinMax<TFrom>> binder) : base(binder, CastFunc, CastToParentFunc)
        {
        }


        public override bool IsConst => parentBinder.IsConst;

        #region static

        private static readonly Func<MinMax<TFrom>, MinMax<TTo>> CastFunc;
        private static readonly Func<MinMax<TFrom>, MinMax<TTo>, MinMax<TFrom>> CastToParentFunc;

        static CastMinMaxBinder()
        {
            var valueCastFunc = CastBinder<TFrom,TTo>.CastFunc;
            CastFunc = minMaxFrom => MinMax.Create(valueCastFunc(minMaxFrom.min), valueCastFunc(minMaxFrom.max));
            
            var valueCastToParentFunc = CastBinder<TFrom,TTo>.CastToParentFunc;
            CastToParentFunc = (minMaxFrom, minMaxTo) =>
            {
                minMaxFrom.min = valueCastToParentFunc(minMaxFrom.min, minMaxTo.min);
                minMaxFrom.max = valueCastToParentFunc(minMaxFrom.max, minMaxTo.max);

                return minMaxFrom;
            };
        }

        #endregion
    }
}