using RosettaUI.Reactive;
using UnityEngine.UIElements;

namespace RosettaUI
{
    public abstract class RangeFieldElement<T, TRange> : FieldBaseElement<T>
    {
        private readonly IGetter<TRange> _minGetter;
        private readonly IGetter<TRange> _maxGetter;

        public readonly ReactiveProperty<TRange> minRx;
        public readonly ReactiveProperty<TRange> maxRx;

        public RangeFieldElement(LabelElement label, IBinder<T> binder, IGetter<MinMax<TRange>> minMaxGetter)
            : this(label, binder, 
                Getter.Create(() => minMaxGetter.Get().min),
                Getter.Create(() => minMaxGetter.Get().max))
        {
        }

        public RangeFieldElement(LabelElement label, IBinder<T> binder, IGetter<TRange> minGetter,
            IGetter<TRange> maxGetter)
            : base(label, binder)
        {
            _minGetter = minGetter;
            _maxGetter = maxGetter;

            minRx = new ReactiveProperty<TRange>(_minGetter.Get());
            maxRx = new ReactiveProperty<TRange>(_maxGetter.Get());
        }

        public TRange Min => minRx.Value;
        public TRange Max => maxRx.Value;

        public bool IsMinConst => _minGetter.IsConst;
        public bool IsMaxConst => _maxGetter.IsConst;

        public override void Update()
        {
            base.Update();
            if (!IsMinConst) minRx.Value = _minGetter.Get();
            if (!IsMaxConst) maxRx.Value = _maxGetter.Get();
        }
    }
}