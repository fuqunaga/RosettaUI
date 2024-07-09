using System;
using System.Collections.Generic;


namespace RosettaUI.Reactive
{
    public class ReactiveProperty<T> : Observable<T>
    {
        T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    NotifyValueChanged();
                }
            }
        }


        public ReactiveProperty() { }
        public ReactiveProperty(T value) => _value = value;

        public override T GetNotifyValue() => Value;
    }


    public static class ReactivePropertyExtensions
    {
        public static IDisposable SubscribeAndCallOnce<T>(this ReactiveProperty<T> source, Action<T> onNext)
        {
            onNext?.Invoke(source.Value);
            return source.Subscribe(onNext);
        }
    }
}