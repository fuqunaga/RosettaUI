using System;

namespace RosettaUI
{
    public static class Binder
    {
        public static Binder<T> Create<T>(Func<T> getter, Action<T> setter)
        {
            return new Binder<T>(Getter.Create(getter), setter);
        }

        public static IBinder Create(object obj, Type type)
        {
            var binder = Create(() => obj, o => obj = o);
            var castBinderType = typeof(CastBinder<,>).MakeGenericType(typeof(object), type);

            return Activator.CreateInstance(castBinderType, binder) as IBinder;
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