using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static class CastBinder
    {
        public static IBinder Create(IBinder binder, Type fromType, Type toType)
        {
            var castBinderType = typeof(CastBinder<,>).MakeGenericType(fromType, toType);
            return (IBinder) Activator.CreateInstance(castBinderType, binder);
        }
    }
    
    public class CastBinder<TFrom, TTo> : ConvertBinder<TFrom, TTo>
    {
        public CastBinder(IBinder<TFrom> binder) : base(binder)
        {
        }

        protected override TTo GetFromParent(TFrom parent) => CastFunc(parent);
        protected override TFrom SetToParent(TFrom parent, TTo value) => CastToParentFunc(parent, value);

        
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