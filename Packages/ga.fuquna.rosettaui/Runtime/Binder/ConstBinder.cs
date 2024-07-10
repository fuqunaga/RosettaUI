using System;

namespace RosettaUI
{
    public static class ConstBinder
    {
        public static Binder<T> Create<T>(T obj) => new Binder<T>(ConstGetter.Create(obj),null);

        public static IBinder Create(object obj, Type type)
        {
            var getter = ConstGetter.Create(obj, type);

            var binderType = typeof(Binder<>).MakeGenericType(type);

            return Activator.CreateInstance(binderType, getter, null) as IBinder;
        }
    }
}