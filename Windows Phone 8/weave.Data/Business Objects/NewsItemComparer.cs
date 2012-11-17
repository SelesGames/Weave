using System;
using System.Collections.Generic;

namespace weave
{
    internal class NewsItemComparer : IEqualityComparer<NewsItem>
    {
        public bool Equals(NewsItem x, NewsItem y)
        {
            return x.Title.Equals(y.Title);
        }

        public int GetHashCode(NewsItem obj)
        {
            return obj.Title.GetHashCode();
        }

        bool RelaxedDateTimeEquality(DateTime x, DateTime y) // if it is 10 minutes diff or less, treat as equal
        {
            return Math.Abs((x - y).TotalHours) <= 48d;
        }
    }
}
