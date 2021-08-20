using System;

namespace Comugi
{
    public static class Binder
    {
        public static Binder<T> Create<T>(Func<T> getter, Action<T> setter) => new Binder<T>(Getter.Create(getter), setter);

    }


    public class Binder<T> : BinderBase<T>
    {
        readonly Action<T> setter;

        public Binder(IGetter<T> getter, Action<T> setter) : base(getter)
        {
            this.setter = setter;
        }

        public override bool IsReadOnly => setter == null && base.IsReadOnly;

        protected override void SetInternal(T t) => setter?.Invoke(t);
    }
}