using System;

namespace Comugi
{
    /// <summary>
    /// 値を持ち外部と同期するElement
    /// </summary>
    public abstract class ReadOnlyValueElement<T> : Element
    {
        #region For Builder

        public event Action<T> setValueToView;
        public T GetInitialValue() => getter.Get();

        #endregion


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
    }
}