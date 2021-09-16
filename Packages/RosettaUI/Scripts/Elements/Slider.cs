using RosettaUI.Reactive;

namespace RosettaUI
{
    public abstract class Slider<T> : FieldBaseElement<T>
    {
        #region For Builder

        public readonly ReactiveProperty<(T, T)> minMaxRx;

        #endregion


        readonly IGetter<(T, T)> minMaxGetter;

        public (T, T) minMax => minMaxRx.Value;
        
        public bool IsMinMaxConst => minMaxGetter.IsConst;


        public Slider(LabelElement label, BinderBase<T> binder, IGetter<(T,T)> minMaxGetter) : base(label, binder)
        {
            this.minMaxGetter = minMaxGetter;
            minMaxRx = new ReactiveProperty<(T, T)>(minMaxGetter.Get());
        }

        public override void Update()
        {
            base.Update();
            if (!IsMinMaxConst)
            {
                minMaxRx.Value = minMaxGetter.Get();
            }
        }
    }
}