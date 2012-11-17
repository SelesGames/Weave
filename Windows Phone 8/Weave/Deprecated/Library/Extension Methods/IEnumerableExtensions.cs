using System.Collections.Generic;

namespace System.Linq
{
    public static partial class IEnumerableExtensions
    {
        public static IEnumerable<T> RandomlySort<T>(this IEnumerable<T> source)
        {
            Random r = new Random();
            return source.Select(o => new { o, i = r.Next() })
                .OrderBy(o => o.i)
                .Select(o => o.o);
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
