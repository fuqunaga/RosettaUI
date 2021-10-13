using System;

namespace RosettaUI
{
    /// <summary>
    ///     Binder, which targets a portion of the parents
    /// </summary>
    public abstract class ChildBinderBase<TParent, TValue> : BinderBase<TValue>
    {
        protected readonly IBinder<TParent> parentBinder;

        protected ChildBinderBase(IBinder<TParent> parentBinder)
        {
            this.parentBinder = parentBinder;
            getter = Getter.Create(() => GetFromChild(parentBinder.Get()));
        }


        public override bool IsNull => parentBinder.IsNull || base.IsNull;
        public override bool IsNullable => parentBinder.IsNullable || base.IsNullable;

        public override bool IsReadOnly => false;

        protected abstract TValue GetFromChild(TParent parent);
        protected abstract TParent SetToParent(TParent parent, TValue value);

        protected override void SetInternal(TValue value)
        {
            var parent = parentBinder.Get();
            parent = SetToParent(parent, value);
            parentBinder.Set(parent);
        }
    }

    public class ChildBinder<TParent, TValue> : ChildBinderBase<TParent, TValue>
    {
        private readonly Func<TParent, TValue> _getter;
        private readonly Func<TParent, TValue, TParent> _setter;

        public ChildBinder(IBinder<TParent> parentBinder, Func<TParent, TValue> getter, Func<TParent, TValue, TParent> setter) 
            : base(parentBinder)
        {
            _getter = getter;
            _setter = setter;
        }

        protected override TValue GetFromChild(TParent parent)
        {
            return _getter(parent);
        }

        protected override TParent SetToParent(TParent parent, TValue value)
        {
            return _setter(parent, value);
        }
    }
}