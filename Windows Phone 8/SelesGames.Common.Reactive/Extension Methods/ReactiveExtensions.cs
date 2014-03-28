using System.Reactive.Disposables;

namespace System.Reactive.Linq
{
    public static class DisposableExtensions
    {
        public static void DisposeWith(this IDisposable observable, CompositeDisposable disposables)
        {
            disposables.Add(observable);
        }
    }

    public static class ReactiveExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action onNext)
        {
            return source.Subscribe(_ => onNext());
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action onNext, Action<Exception> onError)
        {
            return source.Subscribe(_ => onNext(), onError);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action onNext, Action<Exception> onError, Action onCompleted)
        {
            return source.Subscribe(_ => onNext(), onError, onCompleted);
        }
    }
}
