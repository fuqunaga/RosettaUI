using System;
using System.Linq;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class GetterMinMaxExtensions
    {
        public static bool IsMinMax(this IGetter getter)
        {
            return getter.ValueType.GetGenericTypeDefinition() == typeof(MinMax<>);
        }
        
        public static Type GetMinMaxValueType(this IGetter getter)
        {
            Assert.IsTrue(getter.IsMinMax());
            return getter.ValueType.GetGenericArguments().First();
        }
    }
}