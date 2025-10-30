using System;


namespace RosettaUI.Reactive
{
    public static class ObservableExtension
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(new Observer<T>(onNext));
        }
    }
}