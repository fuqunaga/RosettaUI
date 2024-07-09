using System;

namespace RosettaUI
{
    public static class Getter
    {
        public static Getter<T> Create<T>(Func<T> func) => new(func);
        
        public static bool CacheEnable { get; private set; }
        public static uint CacheId { get; private set; }

        public static void EnableCache()
        {
            CacheEnable = true;
            CacheId++;
        }

        public static void DisableCache()
        {
            CacheEnable = false;
        }
    }

    public class Getter<T> : GetterBase<T>
    {
        readonly Func<T> _func;
        
        public Getter(Func<T> func) => _func = func;

        protected override T GetRaw() => (_func != null) ? _func() : default;

        public override bool IsNull => Get() == null;

        public override bool IsNullable => !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null;
        
        public override bool IsConst => false;
        
        public override Type ValueType => typeof(T);
    }
}