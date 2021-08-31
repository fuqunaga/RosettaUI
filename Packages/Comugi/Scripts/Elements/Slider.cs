using System;

namespace RosettaUI
{
    public abstract class Slider<T> : FieldBaseElement<T>
    {
        public Slider(LabelElement label, BinderBase<T> binder, IGetter<(T,T)> minMaxGetter) : base(label, binder)
        {
            this.minMaxGetter = minMaxGetter;
        }

        public override void Update()
        {
            base.Update();
            if (!IsMinMaxConst)
            {
                setMinMaxToView(minMaxGetter.Get());
            }
        }


        #region Internal

        readonly IGetter<(T, T)> minMaxGetter;
        public bool IsMinMaxConst => minMaxGetter.IsConst;
        Action<(T, T)> setMinMaxToView;

        public void RegisterSetMinMaxToView(Action<(T,T)> action) => setMinMaxToView = action;
        public (T, T) GetInitialMinMax() => minMaxGetter.Get();

        #endregion
    }
}