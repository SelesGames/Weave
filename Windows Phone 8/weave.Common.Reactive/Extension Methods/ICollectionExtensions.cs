
namespace System.Collections.Generic
{
    public static class ICollectionExtensions
    {
        public static void SetSource<T>(this ICollection<T> coll, IEnumerable<T> source)
        {
            if (source == null || coll == null)
                return;
            coll.Clear();
            foreach (var s in source)
                coll.Add(s);
        }
    }
}