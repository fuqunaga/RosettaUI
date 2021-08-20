using System;

namespace Comugi
{
    public static class Getter
    {
        public static Getter<T> Create<T>(Func<T> func) => new Getter<T>(func);
    }

    public class Getter<T> : IGetter<T>
    {
        readonly Func<T> func;

        public virtual bool IsConst => false;


        public Getter(Func<T> func) => this.func = func;

        public T Get() => (func != null) ? func() : default;

        public Type ValueType => typeof(T);

        public bool IsNull => Get() == null;

        public bool IsNullable => typeof(T).IsClass;
    }
}