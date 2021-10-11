using RosettaUI.Reactive;

namespace RosettaUI
{
    public abstract class RangeFieldElement<T, TRange> : FieldBaseElement<T>
    {
        private readonly IGetter<(TRange, TRange)> _minMaxGetter;
        public readonly ReactiveProperty<(TRange, TRange)> minMaxRx;


        public RangeFieldElement(LabelElement label, BinderBase<T> binder, IGetter<(TRange, TRange)> minMaxGetter) :
            base(label, binder)
        {
            _minMaxGetter = minMaxGetter;
            minMaxRx = new ReactiveProperty<(TRange, TRange)>(minMaxGetter.Get());
        }

        public (TRange, TRange) MinMax => minMaxRx.Value;

        public bool IsMinMaxConst => _minMaxGetter.IsConst;

        public override void Update()
        {
            base.Update();
            if (!IsMinMaxConst) minMaxRx.Value = _minMaxGetter.Get();
        }
    }
}