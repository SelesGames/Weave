
namespace System.Collections.Generic
{
    public static class IListExtensions
    {
        public static IList<T> AddAndReturn<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        public static List<T> AddAndReturn<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list;
        }
    }
}
