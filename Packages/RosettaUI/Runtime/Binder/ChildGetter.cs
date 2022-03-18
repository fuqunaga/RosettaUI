using System;

namespace RosettaUI
{
    /// <summary>
    ///     Getter, which targets a portion of the parents
    /// </summary>
    public abstract class ChildGetter<TParent, TValue> : GetterBase<TValue>
    {
        protected readonly IGetter<TParent> parentGetter;

        protected ChildGetter(IGetter<TParent> parentGetter)
        {
            this.parentGetter = parentGetter;
        }

        protected abstract TValue GetFromChild(TParent get);


        protected override TValue GetRaw() => GetFromChild(parentGetter.Get());

        public override bool IsNull => parentGetter.IsNull;
        public override bool IsNullable => parentGetter.IsNullable;
        public override bool IsConst => parentGetter.IsConst;
        public override Type ValueType => typeof(TValue);
    }
}