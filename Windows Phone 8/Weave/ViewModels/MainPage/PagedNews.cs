using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave
{
    public class PagedNewsItems : IPagedNewsItems
    {
        List<AsyncNewsList> newsLists = new List<AsyncNewsList>();
        NewsItemGroup vm;

        public int PageSize { get; private set; }
        public int NumberOfPagesToTakeAtATime { get; private set; }
        public int PageCount { get; private set; }
        public int TotalNewsCount { get; private set; }
        public int NewNewsCount { get; private set; }

        public PagedNewsItems(NewsItemGroup vm, int pageSize, int numberOfPagesToTakeAtATime)
        {
            this.vm = vm;
            PageSize = pageSize;
            NumberOfPagesToTakeAtATime = numberOfPagesToTakeAtATime;
        }

        public async Task Refresh(EntryType entry)
        {
            var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
            var currentNewsList = await vm.GetNewsList(entry, 0, takeAmount);
            PageCount = currentNewsList.GetPageCount(PageSize);
            TotalNewsCount = currentNewsList.TotalArticleCount;
            NewNewsCount = currentNewsList.NewArticleCount;

            for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
            {
                var skipMult = j;
                var t = new AsyncNewsList
                {
                    News = () => Task.FromResult(currentNewsList.News.Skip(skipMult * PageSize).Take(PageSize).ToList()),
                };

                newsLists.Add(t);
            }

            Chunk();
        }

        void Chunk()
        {
            for (int i = NumberOfPagesToTakeAtATime; i < PageCount; i += NumberOfPagesToTakeAtATime)
            {
                var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
                var skipAmount = i * PageSize;
                var load = Lazy.Create(() => vm.GetNewsList(EntryType.Peek, skipAmount, takeAmount));

                for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
                {
                    var skipMult = j;
                    var t = new AsyncNewsList
                    {
                        News = async () => (await load.Get()).News.Skip(skipMult * PageSize).Take(PageSize).ToList(),
                    };

                    newsLists.Add(t);
                }
            }
        }

        public AsyncNewsList GetNewsFuncForPage(int page)
        {
            return newsLists[page];
        }
    }
}
