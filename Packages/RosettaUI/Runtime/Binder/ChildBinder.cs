namespace RosettaUI
{
    /// <summary>
    /// Binder, which targets a portion of the parents
    /// </summary>
    public abstract class ChildBinder/*Base*/<TParent, TValue> : BinderBase<TValue>
    {
        protected readonly IBinder<TParent> parentBinder;

        protected ChildBinder/*Base*/(IBinder<TParent> parentBinder)
        {
            this.parentBinder = parentBinder;
            getter = Getter.Create(() => GetFromParent(parentBinder.Get()));
        }


        public override bool IsNull => parentBinder.IsNull || base.IsNull;
        public override bool IsNullable => parentBinder.IsNullable || base.IsNullable;

        public override bool IsReadOnly => false;

        protected abstract TValue GetFromParent(TParent parent);
        protected abstract TParent SetToParent(TParent parent, TValue value);

        protected override void SetInternal(TValue value)
        {
            var parent = parentBinder.Get();
            parent = SetToParent(parent, value);
            parentBinder.Set(parent);
        }
    }
}