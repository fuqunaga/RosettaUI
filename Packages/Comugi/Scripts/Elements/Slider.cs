using System;

namespace Comugi
{
    public abstract class Slider<T> : ValueElement<T>
    {
        public Slider(BinderBase<T> binder, IGetter<(T,T)> minMaxGetter) : base(binder)
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