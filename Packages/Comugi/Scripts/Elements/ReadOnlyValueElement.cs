using System;

namespace Comugi
{
    /// <summary>
    /// 値を持ち外部と同期するElement
    /// </summary>
    public abstract class ReadOnlyValueElement<T> : Element
    {
        readonly IGetter<T> getter;
        public bool IsConst => getter.IsConst;

        public ReadOnlyValueElement(IGetter<T> getter)
        {
            this.getter = getter ?? new ConstGetter<T>(default);
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (!IsConst)
            {
                setValueToView?.Invoke(getter.Get());
            }
        }


        #region Internal

        Action<T> setValueToView;

        public void RegisterSetValueToView(Action<T> action) => setValueToView = action;

        public T GetInitialValue() => getter.Get();

        #endregion
    }
}