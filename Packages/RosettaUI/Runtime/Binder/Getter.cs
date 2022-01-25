using System;

namespace RosettaUI
{
    public static class Getter
    {
        public static Getter<T> Create<T>(Func<T> func) => new(func);
    }

    public class Getter<T> : GetterBase<T>
    {
        readonly Func<T> _func;
        
        public Getter(Func<T> func) => _func = func;

        protected override T GetRaw() => (_func != null) ? _func() : default;

        public override bool IsNull => Get() == null;

        public override bool IsNullable => typeof(T).IsClass;
        
        public override bool IsConst => false;
        
        public override Type ValueType => typeof(T);
    }
}