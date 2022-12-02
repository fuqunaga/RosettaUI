using System;

namespace RosettaUI
{
    /// <summary>
    /// 値を持ち外部と同期するElement
    /// </summary>
    public abstract class ReadOnlyValueElement<T> : Element
    {
        public readonly IGetter<T> getter;
        internal event Action<T> listenValue;

        private ReadOnlyValueViewBridgeBase _viewBridgeBase;
        
        public T Value => getter.Get();
        
        public bool IsConst => getter.IsConst;

        protected ReadOnlyValueElement(IGetter<T> getter)
        {
            this.getter = getter ?? new ConstGetter<T>(default);
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (!IsConst)
            {
                listenValue?.Invoke(getter.Get());
            }
        }

        private void ClearListenValue() => listenValue = null;

        protected override ElementViewBridge CreateViewBridge() => new ReadOnlyValueViewBridgeBase(this);

        public class ReadOnlyValueViewBridgeBase : ElementViewBridge
        {
            private ReadOnlyValueElement<T> Element => (ReadOnlyValueElement<T>)element;
            
            public ReadOnlyValueViewBridgeBase(ReadOnlyValueElement<T> element) : base(element)
            {
            }

            public void SubscribeValueOnUpdateCallOnce(Action<T> action)
            {
                if (action == null) return;
                action.Invoke(Element.Value);
                SubscribeValueOnUpdate(action);
            }

            public void SubscribeValueOnUpdate(Action<T> action)
            {
                if (action != null && !Element.IsConst)
                {
                    Element.listenValue += action;
                }
            }

            public override void UnsubscribeAll()
            {
                base.UnsubscribeAll();
                Element.ClearListenValue();
            } 
        }
    }
    
        
    public static partial class ElementViewBridgeExtensions
    {
        public static ReadOnlyValueElement<T>.ReadOnlyValueViewBridgeBase GetViewBridge<T>(this ReadOnlyValueElement<T> element) => (ReadOnlyValueElement<T>.ReadOnlyValueViewBridgeBase)element.ViewBridge;
    }
}