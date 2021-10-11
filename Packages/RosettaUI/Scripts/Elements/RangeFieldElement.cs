using RosettaUI.Reactive;

namespace RosettaUI
{
    public abstract class RangeFieldElement<T, TRange> : FieldBaseElement<T>
    {
        private readonly IMinMaxGetter<TRange> _minMaxGetter;
        public readonly ReactiveProperty<MinMax<TRange>> minMaxRx;


        public RangeFieldElement(LabelElement label, BinderBase<T> binder, IMinMaxGetter<TRange> minMaxGetter) :
            base(label, binder)
        {
            _minMaxGetter = minMaxGetter;
            minMaxRx = new ReactiveProperty<MinMax<TRange>>(minMaxGetter.Get());
        }

        public MinMax<TRange> MinMax => minMaxRx.Value;

        public bool IsMinMaxConst => _minMaxGetter.IsConst;

        public override void Update()
        {
            base.Update();
            if (!IsMinMaxConst) minMaxRx.Value = _minMaxGetter.Get();
        }
    }
}