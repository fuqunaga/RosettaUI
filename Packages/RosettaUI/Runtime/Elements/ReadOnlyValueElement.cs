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

        public ViewBridge GetViewBridge() => new(this);

        public readonly struct ViewBridge
        {
            private readonly ReadOnlyValueElement<T> _element;
            public ViewBridge(ReadOnlyValueElement<T> element) => _element = element;

            public void SubscribeValueOnUpdateCallOnce(Action<T> action)
            {
                if (action == null) return;
                action.Invoke(_element.Value);
                SubscribeValueOnUpdate(action);
            }

            public void SubscribeValueOnUpdate(Action<T> action)
            {
                if (action != null && !_element.IsConst)
                {
                    _element.listenValue += action;
                }
            }

            public void UnsubscribeAll() => _element.ClearListenValue();
        }
    }
}