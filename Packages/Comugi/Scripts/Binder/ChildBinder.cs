using System;

namespace Comugi
{
    /// <summary>
    /// Binder, which targets a portion of the parents 
    /// </summary>
    public class ChildBinder<TParent, TValue> : BinderBase<TValue>
    {
        protected BinderBase<TParent> parentBinder;
        protected Func<TParent, TValue, TParent> setter;


        public ChildBinder(BinderBase<TParent> parentBinder, Func<TParent, TValue> getter, Func<TParent, TValue, TParent> setter)
            : base(Getter.Create(() => getter(parentBinder.Get())))
        {
            this.parentBinder = parentBinder;
            this.setter = setter;
        }

        public ChildBinder(BinderBase<TParent> parentBinder, (Func<TParent, TValue>, Func<TParent, TValue, TParent>) pair) : this(parentBinder, pair.Item1, pair.Item2) { }


        public override bool IsNull => parentBinder.IsNull || base.IsNull;
        public override bool IsNullable => parentBinder.IsNullable || base.IsNullable;

        public override bool IsReadOnly => false;

        protected override void SetInternal(TValue value)
        {
            var parent = parentBinder.Get();
            parent = setter(parent, value);
            parentBinder.Set(parent);
        }
    }
}