using System;

namespace RosettaUI
{
    public static class Getter
    {
        public static Getter<T> Create<T>(Func<T> func) => new Getter<T>(func);
    }

    public class Getter<T> : IGetter<T>
    {
        readonly Func<T> _func;

        public virtual bool IsConst => false;


        public Getter(Func<T> func) => _func = func;

        public T Get() => (_func != null) ? _func() : default;

        public Type ValueType => typeof(T);

        public bool IsNull => Get() == null;

        public bool IsNullable => typeof(T).IsClass;
    }
}