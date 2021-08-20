namespace Comugi
{
    /// <summary>
    /// 値を持ち外部と同期するElement
    /// </summary>
    public class ValueElement<T> : ReadOnlyValueElement<T>
    {
       readonly BinderBase<T> binder;

        public ValueElement(BinderBase<T> binder) : base(binder) => this.binder = binder;


        #region Internal

        public void OnViewValueChanged(T t) => binder?.Set(t);

        #endregion
    }
}