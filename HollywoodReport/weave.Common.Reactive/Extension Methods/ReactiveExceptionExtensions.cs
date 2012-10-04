using System.Reactive.Disposables;

namespace System.Reactive.Linq
{
    public static class ReactiveExceptionExtensions
    {
        //public static IObservable<T> AsObservable<T>(this Exception e)
        //{
        //    var subject = new AsyncSubject<T>();
        //    subject.OnError(e);
        //    return subject.AsObservable();
        //}

        public static IObservable<T> AsObservable<T>(this Exception e)
        {
            return Observable.Create<T>(observer =>
            {
                observer.OnError(e);
                return Disposable.Empty;
            });
        }
    }
}
