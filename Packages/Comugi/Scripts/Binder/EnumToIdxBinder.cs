using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Comugi
{
    public class EnumToIdxBinder<TFrom> : ChildBinder<TFrom, int>
    {
        #region static

        readonly static Func<TFrom, int> toIdxFunc;
        readonly static Func<TFrom, int, TFrom> toEnumFunc;

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

            toIdxFunc = (e) =>
            {
                valueToIdx.TryGetValue(e, out var ret);
                return ret;
            };

            toEnumFunc = (e, idx) =>
            {
                return (0 <= idx && idx < idxToValue.Count)
                    ? idxToValue[idx]
                    : e;
            };

        }

        #endregion


        public EnumToIdxBinder(BinderBase<TFrom> parentBinder) : base(parentBinder, toIdxFunc, toEnumFunc)
        {
        }


        public override bool IsConst => parentBinder.IsConst;
    }
}