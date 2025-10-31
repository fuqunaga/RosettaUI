using System;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class NullableToValueBinder
    {
        public static IBinder Create(IBinder binder)
        {
            var parentType = binder.ValueType;
            Assert.IsTrue(TypeUtility.IsNullable(parentType));
            
            var type = parentType.GetGenericArguments()[0];
            var binderType = typeof(NullableToValueBinder<>).MakeGenericType(type);

            return Activator.CreateInstance(binderType, binder) as IBinder;
        }
    }
    
    public class NullableToValueBinder<T> : ConvertBinder<T?, T>
        where T : struct
    {
        public NullableToValueBinder(IBinder<T?> binder) : base(binder)
        {
        }

        protected override T GetFromParent(T? parent) => parent ?? default;
        protected override T? SetToParent(T? parent, T value) => value;
    }
}