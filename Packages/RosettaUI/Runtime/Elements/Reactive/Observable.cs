using System;
using System.Collections.Generic;

namespace RosettaUI.Reactive
{
    public abstract class Observable<T> : IObservable<T>
    {
        List<IObserver<T>> _observers;

        public abstract T GetNotifyValue();
        
        protected void NotifyValueChanged()
        {
            if (_observers != null)
            {
                var value = GetNotifyValue();
                foreach (var observer in _observers)
                {
                    observer.OnNext(value);
                }
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers ??= new List<IObserver<T>>();
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new UnsubscribeObj(_observers, observer);
        }

        private class UnsubscribeObj : IDisposable
        {
            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            public UnsubscribeObj(List<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }

    public static class ObservableSubscribeAndCallOnceExtension
    {
        public static IDisposable SubscribeAndCallOnce<T>(this Observable<T> observable, Action<T> onNext)
        {
            onNext?.Invoke(observable.GetNotifyValue());
            return observable.Subscribe(onNext);
        }
    }
}