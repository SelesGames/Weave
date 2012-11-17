using System.Linq;

namespace System.Collections.Generic
{
    internal class FILOQueueWrapper<T> : IEnumerable<T>
    {
        List<T> list;
        int capacity;




        #region Constructors

        public FILOQueueWrapper(List<T> list, int capacity)
        {
            if (list == null)
                throw new ArgumentNullException("list cannot be null in constructor of FILOQueueWrapper");

            this.list = list;
            this.capacity = capacity;
            Trim();
        }

        #endregion




        public void QueueUnique(T item, IEqualityComparer<T> comparer)
        {
            if (!list.Contains(item, comparer))
                list.Add(item);
            Trim();
        }

        public T Dequeue()
        {
            if (!list.Any())
                throw new InvalidOperationException("FILOQueue is empty");

            var lastIndex = list.Count - 1;
            var item = list[lastIndex];
            list.RemoveAt(lastIndex);
            return item;
        }

        void Trim()
        {
            while (list.Count > capacity)
                list.RemoveAt(0);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (list == null) return null;
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (list == null) return null;
            return list.GetEnumerator();
        }

        public int Count { get { return list == null ? 0 : list.Count; } }
    }
}