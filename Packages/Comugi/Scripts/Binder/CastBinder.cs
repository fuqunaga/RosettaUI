using System;
using System.Linq.Expressions;

namespace Comugi
{
    public class CastBinder<TFrom, TTo> : ChildBinder<TFrom, TTo>
    {
        #region static

        readonly static Func<TFrom, TTo> castFunc;
        readonly static Func<TFrom, TTo, TFrom> castToParentFunc;

        static CastBinder()
        {
            var from = Expression.Parameter(typeof(TFrom));
            var cast = Expression.Convert(from, typeof(TTo));

            castFunc = Expression.Lambda<Func<TFrom, TTo>>(cast, from).Compile();

            var to = Expression.Parameter(typeof(TTo));
            var castToParent = Expression.Convert(to, typeof(TFrom));

            castToParentFunc = Expression.Lambda<Func<TFrom, TTo, TFrom>>(castToParent, from, to).Compile();
        }

        #endregion


        public CastBinder(BinderBase<TFrom> binder) : base(binder, castFunc, castToParentFunc)
        {
        }


        public override bool IsConst => parentBinder.IsConst;
    }
}