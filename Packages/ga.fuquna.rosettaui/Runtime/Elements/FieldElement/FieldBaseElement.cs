using RosettaUI.UndoSystem;

namespace RosettaUI
{
    /// <summary>
    /// 値を持ち外部と同期するElement
    /// </summary>
    public abstract class FieldBaseElement<T> : ReadOnlyFieldElement<T>, IUndoRestoreElement
    {
        public FieldOption Option { get; }
        
        // ReSharper disable once MemberCanBeProtected.Global
        public bool ShouldRecordUndo { get; set; } = true;
        
        
        private readonly IBinder<T> _binder;
        
        protected FieldBaseElement(LabelElement label, IBinder<T> binder, in FieldOption option = default) : base(label, binder)
        {
            _binder = binder;
            Option = option;
            Interactable = !binder.IsReadOnly;
        }
        
        
        #region IUndoRestoreElement
        
        public IElementRestoreRecord CreateRestoreRecord() => FieldBaseElementRestoreRecord<T>.Create(this);
        
        #endregion

        
        protected override ElementViewBridge CreateViewBridge() => new FieldViewBridgeBase(this);
        
        
        public class FieldViewBridgeBase : ReadOnlyValueViewBridgeBase
        {
            private FieldBaseElement<T> Element => (FieldBaseElement<T>)element; 
            
            public FieldViewBridgeBase(FieldBaseElement<T> element) : base(element)
            {
            }
            
            public void SetValueFromView(T value)
            {
                if (Element.ShouldRecordUndo)
                {
                    var before = Element.Value;
                    UndoRecordFieldBaseElement<T>.Record(Element, before, value);
                }
                
                Element._binder?.Set(value);
                Element.NotifyViewValueChanged();
            }
        }
    }
    
            
    public static partial class ElementViewBridgeExtensions
    {
        public static FieldBaseElement<T>.FieldViewBridgeBase GetViewBridge<T>(this FieldBaseElement<T> element) => (FieldBaseElement<T>.FieldViewBridgeBase)element.ViewBridge;
    }
}