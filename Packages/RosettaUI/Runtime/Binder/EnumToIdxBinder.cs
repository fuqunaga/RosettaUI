using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public class EnumToIdxBinder<TFrom> : ChildBinder<TFrom, int>
    {
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


        public EnumToIdxBinder(IBinder<TFrom> parentBinder) : base(parentBinder, ToIdxFunc, ToEnumFunc)
        {
        }


        public override bool IsConst => parentBinder.IsConst;
    }
}