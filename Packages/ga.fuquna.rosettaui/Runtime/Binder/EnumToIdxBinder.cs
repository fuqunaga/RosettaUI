using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class EnumToIdxBinder
    {
        public static IBinder<int> Create(IBinder binder)
        {
            var valueType = binder.ValueType;
            var binderType = typeof(EnumToIdxBinder<>).MakeGenericType(valueType);
            return Activator.CreateInstance(binderType, binder) as IBinder<int>;
        }
    }
    
    public class EnumToIdxBinder<TFrom> : ConvertBinder<TFrom, int>
    {
        public EnumToIdxBinder(IBinder<TFrom> parentBinder) : base(parentBinder)
        {
        }

        protected override int GetFromParent(TFrom parent) => ToIdxFunc(parent);
        protected override TFrom SetToParent(TFrom parent, int value) => ToEnumFunc(parent, value);


        #region static

        static readonly Func<TFrom, int> ToIdxFunc;
        static readonly Func<TFrom, int, TFrom> ToEnumFunc;

        static EnumToIdxBinder()
        {
            Assert.IsTrue(typeof(TFrom).IsEnum);
            var idxToValue = Enum.GetValues(typeof(TFrom)).Cast<TFrom>().ToList();
            var valueToIdx = new Dictionary<TFrom, int>();
            for(var i=0; i<idxToValue.Count; ++i)
            {
                var v = idxToValue[i];
                valueToIdx[v] = i;
            }

            ToIdxFunc = (e) =>
            {
                valueToIdx.TryGetValue(e, out var ret);
                return ret;
            };

            ToEnumFunc = (e, idx) =>
            {
                return (0 <= idx && idx < idxToValue.Count)
                    ? idxToValue[idx]
                    : e;
            };

        }

        #endregion


    }
}