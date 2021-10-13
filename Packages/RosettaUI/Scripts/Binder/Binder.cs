using System;

namespace RosettaUI
{
    public static class Binder
    {
        public static Binder<T> Create<T>(Func<T> getter, Action<T> setter)
        {
            return new Binder<T>(Getter.Create(getter), setter);
        }
    }


    public class Binder<T> : BinderBase<T>
    {
        private readonly Action<T> _setter;

        public Binder(IGetter<T> getter, Action<T> setter) : base(getter)
        {
            _setter = setter;
        }

        public override bool IsReadOnly => _setter == null && base.IsReadOnly;

        protected override void SetInternal(T t)
        {
            _setter?.Invoke(t);
        }
    }
}