using System;
using System.Collections.Generic;

namespace weave
{
    internal class UpdateParametersComparer : IEqualityComparer<UpdateParameters>
    {
        public bool Equals(UpdateParameters x, UpdateParameters y)
        {
            if (x == y)
                return true;

            return
                x != null &&
                y != null &&
                x.Etag == y.Etag &&
                x.LastModified == y.LastModified &&
                x.MostRecentNewsItemPubDate == y.MostRecentNewsItemPubDate &&
                x.NewsHash.Equals(y.NewsHash);
        }

        public int GetHashCode(UpdateParameters obj)
        {
            throw new NotImplementedException();
        }
    }
}
