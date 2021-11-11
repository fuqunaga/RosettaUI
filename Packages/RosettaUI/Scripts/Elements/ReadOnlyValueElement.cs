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
            //valueRx = new ReactiveProperty<T>(this.getter.Get());
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (!IsConst)
            {
                listenValue?.Invoke(getter.Get());
            }
        }
    }
    
    

    public static class ReadOnlyValueElementSubscribe
    {
        public static void SubscribeValueOnUpdateCallOnce<T>(this ReadOnlyValueElement<T> me, Action<T> action)
        {
            action?.Invoke(me.Value);
            me.SubscribeValueOnUpdate(action);
        }
        
        public static void SubscribeValueOnUpdate<T>(this ReadOnlyValueElement<T>me, Action<T> action)
        {
            if (!me.IsConst)
            {
                me.listenValue += action;
            }
            
        }
    }
}