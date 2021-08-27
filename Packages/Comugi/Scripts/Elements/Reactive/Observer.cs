using System;


namespace Comugi.Reactive
{

    public class Observer<T> : IObserver<T>
    {
        Action<T> onNext;
        Action<Exception> onError;
        Action onComplete;

        public Observer(Action<T> onNext = null, Action<Exception> onError = null, Action onComplete = null)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onComplete = onComplete;
        }

        public void OnNext(T value) => onNext?.Invoke(value);
        public void OnError(Exception error) => onError?.Invoke(error);
        public void OnCompleted() => onComplete?.Invoke();
    }
}