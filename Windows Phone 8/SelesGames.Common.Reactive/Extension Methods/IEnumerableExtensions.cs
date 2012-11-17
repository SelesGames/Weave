using System.Collections.Generic;

namespace System.Linq
{
    public static partial class IEnumerableExtensions
    {
        static Random r = new Random();

        public static T Random<T>(this IList<T> list)
        {
            if (list == null) throw new ArgumentNullException("list in IEnumerableExtensions.Random");

            return list[r.Next(0, list.Count)];
        }

        public static IEnumerable<T> RandomlySort<T>(this IEnumerable<T> source)
        {
            return source.Select(o => new { o, i = r.Next() })
                .OrderBy(o => o.i)
                .Select(o => o.o);
        }

        //public static IEnumerable<TResult> Zip<T1, T2, TResult>(this IEnumerable<T1> coll1, IEnumerable<T2> coll2, Func<T1, T2, TResult> selector)
        //{
        //    var enumer1 = coll1.GetEnumerator();
        //    var enumer2 = coll2.GetEnumerator();

        //    while (enumer1.MoveNext() && enumer2.MoveNext())
        //        yield return selector(enumer1.Current, enumer2.Current);
        //}

        public static IEnumerable<T> Do<T>(this IEnumerable<T> o, Action<T> action)
        {
            if (o == null) throw new ArgumentNullException("o in IEnumerableExtensions.Do");

            foreach (var item in o)
            {
                action(item);
                yield return item;
            }
        }
    }
}

namespace System.Collections.ObjectModel
{
    public static partial class IEnumerableExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return null;

            var coll = new ObservableCollection<T>();
            foreach (var o in source)
                coll.Add(o);
            return coll;
        }
    }
}
