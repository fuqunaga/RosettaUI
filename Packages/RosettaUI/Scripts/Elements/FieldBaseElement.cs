namespace RosettaUI
{
    /// <summary>
    /// 値を持ち外部と同期するElement
    /// </summary>
    public abstract class FieldBaseElement<T> : ReadOnlyFieldElement<T>
    {
       readonly BinderBase<T> binder;

        public FieldBaseElement(LabelElement label, BinderBase<T> binder) : base(label, binder)
        {
            this.binder = binder;
            interactable = !binder.IsReadOnly;
        }


        #region Internal

        public void OnViewValueChanged(T t) => binder?.Set(t);

        #endregion
    }
}