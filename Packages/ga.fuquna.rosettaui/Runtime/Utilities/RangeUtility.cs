using System;
using UnityEngine;

namespace RosettaUI
{
    public static class RangeUtility
    {
        public static (IGetter, IGetter) CreateGetterMinMax(RangeAttribute range, Type type)
        {
            if (range == null) return (null, null);

            return (
                CastGetter.Create(ConstGetter.Create(range.min), typeof(float), type),
                CastGetter.Create(ConstGetter.Create(range.max), typeof(float), type)
            );
        }
        
        public static (IGetter<T>, IGetter<T>) CreateGetterMinMax<T>(RangeAttribute range)
        {
            if (range == null) return (null, null);

            return (
                new CastGetter<float, T>(ConstGetter.Create(range.min)),
                new CastGetter<float, T>(ConstGetter.Create(range.max))
            );
        }
    }
}