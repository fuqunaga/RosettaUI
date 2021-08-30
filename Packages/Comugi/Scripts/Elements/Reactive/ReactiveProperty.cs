using System;
using System.Collections.Generic;


namespace RosettaUI.Reactive
{
    public class ReactiveProperty<T> : IObservable<T>
    {
        List<IObserver<T>> _observers;
        T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    NotifyPropertyValue();
                }
            }
        }


        public ReactiveProperty() { }
        public ReactiveProperty(T value) => _value = value;


        public void NotifyPropertyValue()
        {
            if (_observers != null)
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(_value);
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

            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T>> _observers;
            private IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }
}