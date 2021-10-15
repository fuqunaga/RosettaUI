using System;

namespace RosettaUI
{
    /// <summary>
    ///     Getter, which targets a portion of the parents
    /// </summary>
    public abstract class ChildGetterBase<TParent, TValue> : IGetter<TValue>
    {
        protected readonly IGetter<TParent> parentGetter;

        protected ChildGetterBase(IGetter<TParent> parentGetter)
        {
            this.parentGetter = parentGetter;
        }

        protected abstract TValue GetFromChild(TParent get);


        public TValue Get() => GetFromChild(parentGetter.Get());

        public bool IsNull => parentGetter.IsNull;
        public bool IsNullable => parentGetter.IsNull;
        public bool IsConst => parentGetter.IsNull;
        public Type ValueType => typeof(TValue);
    }
}