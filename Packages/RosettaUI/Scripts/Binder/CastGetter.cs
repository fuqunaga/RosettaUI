using System;
using System.Linq.Expressions;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class CastGetter
    {
        public static IGetter<TTo> Create<TFrom, TTo>(IGetter<TFrom> getter)
        {
            return getter == null ? null : new CastGetter<TFrom, TTo>(getter);
        }
        
        public static IGetter Create(IGetter getter, Type fromType, Type toType)
        {
            if (getter == null) return null;
            
            var castGetterType = typeof(CastGetter<,>).MakeGenericType(fromType, toType);
            return (IGetter)Activator.CreateInstance(castGetterType, getter);
        }
    }
    
    
    public class CastGetter<TFrom, TTo> : Getter<TTo>
    {
        private readonly IGetter<TFrom> _parentGetter;
        
        public CastGetter(IGetter<TFrom> parentGetter) : base(() => CastFunc(parentGetter.Get()))
        {
            Assert.IsNotNull(parentGetter);
            _parentGetter = parentGetter;
        }


        public override bool IsConst => _parentGetter.IsConst;

        #region static

        private static readonly Func<TFrom, TTo> CastFunc;

        static CastGetter()
        {
            var from = Expression.Parameter(typeof(TFrom));
            var cast = Expression.Convert(from, typeof(TTo));

            CastFunc = Expression.Lambda<Func<TFrom, TTo>>(cast, from).Compile();
        }

        #endregion
    }
}