using System;


namespace RosettaUI.Reactive
{
    public static class ObservableExtension
    {
#if !NET_4_6
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(new Observer<T>(onNext));
        }
#endif
    }
}