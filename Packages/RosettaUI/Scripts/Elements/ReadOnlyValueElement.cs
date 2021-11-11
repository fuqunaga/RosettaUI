using RosettaUI.Reactive;

namespace RosettaUI
{
    /// <summary>
    /// 値を持ち外部と同期するElement
    /// </summary>
    public abstract class ReadOnlyValueElement<T> : Element
    {
        public readonly ReactiveProperty<T> valueRx;
        public readonly IGetter<T> getter;

        public T Value => valueRx.Value;
        
        public bool IsConst => getter.IsConst;

        protected ReadOnlyValueElement(IGetter<T> getter)
        {
            this.getter = getter ?? new ConstGetter<T>(default);
            valueRx = new ReactiveProperty<T>(this.getter.Get());
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (!IsConst)
            {
                valueRx.Value = getter.Get();
            }
        }
    }
}