using Microsoft.Phone.Controls;
using Weave.ViewModels;

namespace weave
{
    public class TombstoneState
    {
        public NewsItem CurrentlyViewedNewsItem { get; set; }
        public PageOrientation CurrentLockedPageOrientation { get; set; }
        public int ArticleListCurrentPage { get; set; }
    }
}