using System.Collections.Generic;
using System.Disposables;

namespace System.Linq
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
        public static IObservable<IList<T>> BufferUntilTime<T>(this IObservable<T> source, TimeSpan waitTime)
        {
            //var sync = new object();

            return Observable.Create<IList<T>>(observer =>
            {
                var disp = Disposable.Empty;
                IDisposable timerSubRef = null;

                var timer = Observable.Interval(waitTime).Take(1);
                List<T> list = new List<T>();
                bool sourceIsCompleted = false;

                try
                {
                    disp = source.Subscribe(
                        o =>
                        {
                            //lock (sync)
                            //{
                                if (timerSubRef != null)
                                    timerSubRef.Dispose();

                                list.Add(o);

                                timerSubRef = timer.Subscribe(
                                    _ =>
                                    {
                                        //lock (sync)
                                        //{
                                            observer.OnNext(list);

                                            if (!sourceIsCompleted)
                                            {
                                                list = new List<T>();
                                            }
                                            else
                                            {
                                                observer.OnCompleted();
                                            }
                                        //}
                                    },
                                    observer.OnError);
                            //}
                        },
                        observer.OnError,
                        () =>
                        {
                            //lock (sync)
                            //{
                                sourceIsCompleted = true;
                            //}
                        });
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return () =>
                {
                    //lock (sync)
                    //{
                        if (disp != null)
                            disp.Dispose();
                        if (timerSubRef != null)
                            timerSubRef.Dispose();
                    //}
                };
            });
        }


        public static IObservable<T> WithMemory<T>(this IObservable<T> source)
        {
            CompositeDisposable disposables = new CompositeDisposable();
            var cachedResults = new List<Tuple<T, Exception>>();

            try
            {
                var disp = source.Subscribe(
                    result => cachedResults.Add(Tuple.Create<T, Exception>(result, null)),
                    exception => cachedResults.Add(Tuple.Create<T, Exception>(default(T), exception)));
                disposables.Add(disp);
            }
            catch (Exception) { }

            return Observable.CreateWithDisposable<T>(observer =>
            {
                IDisposable disp2 = null;
                try
                {
                    foreach (var result in cachedResults)
                    {
                        if (result.Item2 != null) // there was an error
                            observer.OnError(result.Item2);
                        else
                            observer.OnNext(result.Item1);
                    }

                    disp2 = source.Subscribe(
                        result => observer.OnNext(result),
                        ex => observer.OnError(ex),
                        () => observer.OnCompleted());
                    disposables.Add(disp2);
                }
                catch (Exception e) { observer.OnError(e); }
                return disposables;
            });
        }
    }
}
