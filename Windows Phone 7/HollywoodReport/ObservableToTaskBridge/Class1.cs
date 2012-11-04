using System;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static class Class1
    {
        public static Task<T> ToTask<T>(this IObservable<T> task)
        {
            var t = new TaskCompletionSource<T>();
            task.Subscribe(t.SetResult, t.SetException);
            return t.Task;
            //return Microsoft.Phone.Reactive.ObservableTaskExtensions.ToTask<T>(task);
        }
    }
}
