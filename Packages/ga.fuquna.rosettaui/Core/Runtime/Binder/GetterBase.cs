using System;

namespace RosettaUI
{
    public abstract class GetterBase<T> : IGetter<T>
    {
        private uint _cacheId;
        private T _cache;

        public virtual T Get()
        {
            if (!Getter.CacheEnable) return GetRaw();

            if (Getter.CacheId != _cacheId)
            {
                _cache = GetRaw();
                _cacheId = Getter.CacheId;
            }

            return _cache;
        }

        protected abstract T GetRaw();
        public abstract bool IsNull { get; }
        public abstract bool IsNullable { get; }
        public abstract bool IsConst { get; }
        public abstract Type ValueType { get; }
    }
}