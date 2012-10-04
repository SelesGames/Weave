//using System;
//using System.Collections.Generic;

//namespace weave
//{
//    internal class FeedSourceComparer : IEqualityComparer<FeedSource>
//    {
//        public bool Equals(FeedSource x, FeedSource y)
//        {
//            if (x == null || y == null || x.FeedUri == null || y.FeedUri == null)
//                return false;

//            return x.FeedUri.Equals(y.FeedUri, StringComparison.OrdinalIgnoreCase);
//        }

//        public int GetHashCode(FeedSource obj)
//        {
//            return (obj != null && obj.FeedUri != null) ? obj.FeedUri.GetHashCode() : -1;
//        }

//        //static FeedSourceComparer instance = new FeedSourceComparer();

//        //public static FeedSourceComparer Instance { get { return instance; } }
//    }
//}
