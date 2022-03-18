using System;

namespace RosettaUI.Reactive
{
    public class Observer<T> : IObserver<T>
    {
        readonly Action<T> _onNext;
        readonly Action<Exception> _onError;
        readonly Action _onComplete;

        public Observer(Action<T> onNext = null, Action<Exception> onError = null, Action onComplete = null)
        {
            _onNext = onNext;
            _onError = onError;
            _onComplete = onComplete;
        }

        public void OnNext(T value) => _onNext?.Invoke(value);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnCompleted() => _onComplete?.Invoke();
    }
}