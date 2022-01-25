using System;

namespace RosettaUI
{
    public abstract class GetterBase<T> : IGetter<T>
    {
        private bool _hasCache;
        private T _cache;

        public virtual T Get()
        {
            if (!_hasCache)
            {
                _cache = GetRaw();
                _hasCache = true;
            }
            
            return _cache;
        }

        public virtual void ClearCache() => _hasCache = false;

        protected abstract T GetRaw();
        public abstract bool IsNull { get; }
        public abstract bool IsNullable { get; }
        public abstract bool IsConst { get; }
        public abstract Type ValueType { get; }
    }
}