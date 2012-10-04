using SelesGames;

namespace System
{
    public static class ObservableExtensions
    {
        static ILogger logger;

        static ObservableExtensions()
        {
            try
            {
                logger = ServiceResolver.Get<ILogger>();
            }
            catch { }
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action onNext)
        {
            return source.SafelySubscribe(_ => onNext());
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action onNext, Action<Exception> onError)
        {
            return source.SafelySubscribe(_ => onNext(), onError);
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action onNext, Action onCompleted)
        {
            return source.SafelySubscribe(_ => onNext(), onCompleted);
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action onNext, Action<Exception> onError, Action onCompleted)
        {
            return source.SafelySubscribe(_ => onNext(), onError, onCompleted);
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (onNext == null) throw new ArgumentNullException("onNext");

            return source.SafelySubscribe(onNext, HandleUncaughtException, delegate { });
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (onNext == null) throw new ArgumentNullException("onNext");
            if (onCompleted == null) throw new ArgumentNullException("onCompleted");

            return source.SafelySubscribe(onNext, HandleUncaughtException, onCompleted);
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (onNext == null) throw new ArgumentNullException("onNext");
            if (onError == null) throw new ArgumentNullException("onError");

            return source.SafelySubscribe(onNext, onError, delegate { });
        }

        public static IDisposable SafelySubscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (onNext == null) throw new ArgumentNullException("onNext");
            if (onError == null) throw new ArgumentNullException("onError");
            if (onCompleted == null) throw new ArgumentNullException("onCompleted");

            return source.Subscribe(
                o =>
                {
                    try
                    {
                        onNext(o);
                    }
                    catch (Exception exception)
                    {
                        onError(exception);
                    }
                },
                exception =>
                {
                    try
                    {
                        onError(exception);
                    }
                    catch (Exception unhandledException)
                    {
                        HandleUncaughtException(unhandledException);
                    }
                },
                () =>
                {
                    try
                    {
                        onCompleted();
                    }
                    catch (Exception unhandledException)
                    {
                        HandleUncaughtException(unhandledException);
                    }
                });
        }

        static void HandleUncaughtException(Exception unhandledException)
        {
            if (logger != null)
                logger.Log(unhandledException);
        }
    }

    public interface ILogger
    {
        void Log(Exception exception);
    }
}
