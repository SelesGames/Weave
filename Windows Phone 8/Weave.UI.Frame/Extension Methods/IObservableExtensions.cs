using System;
using System.Threading.Tasks;

namespace Weave.UI.Frame
{
    internal static class IObservableExtensions
    {
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
