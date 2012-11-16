using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static class Class1
    {
        //public static IObservable<T> ToObservable<T>(this Task<T> task)
        //{
        //    return Microsoft.Phone.Reactive.ObservableTaskExtensions.ToObservable<T>(task);
        //}

        public static Task<T> ToTask<T>(this IObservable<T> task)
        {
            //return Microsoft.Phone.Reactive.ObservableTaskExtensions.ToTask<T>(task);
            return null;
        }
    }
}
