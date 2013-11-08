using Microsoft.Phone.Controls;
using Weave.ViewModels;

namespace weave
{
    public class TombstoneState
    {
        //public bool MainPageCurrentPageShouldBeFlushed { get; set; }
        public NewsItem CurrentlyViewedNewsItem { get; set; }
        //public ReadabilityPageViewModel ActiveWebBrowserPageViewModel { get; set; }
        public PageOrientation CurrentLockedPageOrientation { get; set; }
        public int ArticleListCurrentPage { get; set; }
        public ArticleListContext CurrentArticleListContext { get; set; }
    }

    public enum ArticleListContext
    {
        NO_CONTEXT_SELECTED,        // DEFAULT
        Feed,
        Category,
        Favorites,
        Read,
        Archived
    }
}