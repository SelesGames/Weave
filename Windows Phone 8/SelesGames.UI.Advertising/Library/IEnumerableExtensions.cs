using System.Linq;

namespace System.Collections.Generic
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> RepeatEnumerable<T>(this IEnumerable<Tuple<T, int>> o)
        {
            if (o == null)
                throw new ArgumentNullException("parameter in IEnumerableExtensions.Wrap");

            if (!o.Any())
                yield break;

            var enumerator = o.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                for (int i = 0; i < current.Item2; i++)
                    yield return current.Item1;
            }
        }

        public static IEnumerable<T> Wrap<T>(this IEnumerable<T> o)
        {
            if (o == null)
                throw new ArgumentNullException("parameter in IEnumerableExtensions.Wrap");

            if (!o.Any())
                yield break;

            var enumerator = o.GetEnumerator();

            while (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                enumerator = o.GetEnumerator();
            }
        }
    }
}
