using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave
{
    public interface IPagedNewsItems
    {
        int PageSize { get; }
        int PageCount { get; }
        int TotalNewsCount { get; }
        int NewNewsCount { get; }

        Task Refresh(EntryType entry);
        AsyncNewsList GetNewsFuncForPage(int page);
    }
}
