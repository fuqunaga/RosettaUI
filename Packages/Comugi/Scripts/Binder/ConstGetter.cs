using System;

namespace RosettaUI
{
    public static class ConstGetter
    {
        public static ConstGetter<T> Create<T>(T obj) => new ConstGetter<T>(obj);

        public static IGetter Create(object obj, Type type)
        {
            var getterType = typeof(ConstGetter<>).MakeGenericType(type);
            return Activator.CreateInstance(getterType, obj) as IGetter;
        }
    }

    public class ConstGetter<T> : Getter<T>
    {
        public ConstGetter(T obj) : base(() => obj)
        { }

        public override bool IsConst => true;
    }
}