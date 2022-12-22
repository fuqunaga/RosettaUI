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
        
        protected override ElementViewBridge CreateViewBridge() => new FieldViewBridgeBase(this);
        
        public class FieldViewBridgeBase : ReadOnlyValueViewBridgeBase
        {
            private FieldBaseElement<T> Element => (FieldBaseElement<T>)element; 
            
            public FieldViewBridgeBase(FieldBaseElement<T> element) : base(element)
            {
            }

            public void SetValueFromView(T t)
            {
                Element._binder?.Set(t);
                Element.NotifyViewValueChanged();
            }
        }
    }
    
            
    public static partial class ElementViewBridgeExtensions
    {
        public static FieldBaseElement<T>.FieldViewBridgeBase GetViewBridge<T>(this FieldBaseElement<T> element) => (FieldBaseElement<T>.FieldViewBridgeBase)element.ViewBridge;
    }
}