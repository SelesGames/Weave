using System.Collections.Generic;

namespace weave
{
    public class FeedSourceComparer : IEqualityComparer<FeedSource>
    {
        public bool Equals(FeedSource x, FeedSource y)
        {
            return x.FeedUri.Equals(y.FeedUri);
        }

        public int GetHashCode(FeedSource obj)
        {
            return obj.FeedUri.GetHashCode();
        }

        static FeedSourceComparer instance = new FeedSourceComparer();

        public static FeedSourceComparer Instance { get { return instance; } }
    }
}
