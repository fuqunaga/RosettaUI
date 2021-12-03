using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public class CastBinder<TFrom, TTo> : ChildBinder<TFrom, TTo>
    {
        public CastBinder(IBinder<TFrom> binder) : base(binder, CastFunc, CastToParentFunc)
        {
        }


        public override bool IsConst => parentBinder.IsConst;

        #region static

        public static readonly Func<TFrom, TTo> CastFunc;
        public static readonly Func<TFrom, TTo, TFrom> CastToParentFunc;

        static CastBinder()
        {
            var from = Expression.Parameter(typeof(TFrom));
            var cast = Expression.Convert(from, typeof(TTo));

            CastFunc = Expression.Lambda<Func<TFrom, TTo>>(cast, from).Compile();

            var to = Expression.Parameter(typeof(TTo));
            var castToParent = Expression.Convert(to, typeof(TFrom));

            CastToParentFunc = Expression.Lambda<Func<TFrom, TTo, TFrom>>(castToParent, from, to).Compile();
        }

        #endregion
    }
}