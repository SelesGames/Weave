using System.Threading.Tasks;

namespace System
{
    public static class IObservableExtensions
    {
        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, 
            Action onNext, 
            Action<Exception> onError = null, 
            Action onCompleted = null,
            Action<Exception> onUncaughtError = null)
        {
            return source.SafelySubscribe(_ => onNext(), onError, onCompleted);
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, 
            Action<T> onNext, 
            Action<Exception> onError = null, 
            Action onCompleted = null,
            Action<Exception> onUncaughtException = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (onNext == null) throw new ArgumentNullException("onNext");

            return source.Subscribe(
                o =>
                {
                    try
                    {
                        onNext(o);
                    }
                    catch (Exception exception)
                    {
                        if (onError != null)
                            onError(exception);
                    }
                },
                exception =>
                {
                    try
                    {
                        if (onError != null)
                            onError(exception);
                    }
                    catch (Exception unhandledException)
                    {
                        if (onUncaughtException != null)
                            onUncaughtException(unhandledException);
                    }
                },
                () =>
                {
                    try
                    {
                        if (onCompleted != null)
                            onCompleted();
                    }
                    catch (Exception unhandledException)
                    {
                        if (onUncaughtException != null)
                            onUncaughtException(unhandledException);
                    }
                });
        }

        public static Task<T> ToTask<T>(this IObservable<T> observable)
        {
            var hasValue = false;
            var lastValue = default(T);
            var disposable = new System.Reactive.Disposables.SingleAssignmentDisposable();

            var tcs = new TaskCompletionSource<T>();

            try
            {
                disposable.Disposable = observable.Subscribe(
                    value =>
                    {
                        hasValue = true;
                        lastValue = value;
                    },
                    ex =>
                    {
                        tcs.TrySetException(ex);
                        disposable.Dispose();
                    },
                    () =>
                    {
                        if (hasValue)
                            tcs.TrySetResult(lastValue);
                        else
                            tcs.TrySetException(
                                new InvalidOperationException("no elements in this observable sequence - IObservableExtensions.ToTask"));
                        disposable.Dispose();
                    });
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }
    }
}
