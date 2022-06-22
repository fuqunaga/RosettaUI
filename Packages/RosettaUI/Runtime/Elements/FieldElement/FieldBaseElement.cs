namespace RosettaUI
{
    /// <summary>
    ///     値を持ち外部と同期するElement
    /// </summary>
    public abstract class FieldBaseElement<T> : ReadOnlyFieldElement<T>
    {
        private readonly IBinder<T> _binder;

        protected FieldBaseElement(LabelElement label, IBinder<T> binder) : base(label, binder)
        {
            _binder = binder;
            Interactable = !binder.IsReadOnly;
        }
        
        public void OnViewValueChanged(T t)
        {
            _binder?.Set(t);
            NotifyViewValueChanged();
        }
    }
}