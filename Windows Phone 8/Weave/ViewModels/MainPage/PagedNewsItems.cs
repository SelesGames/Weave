using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.WP.ViewModels.GroupedNews;

namespace Weave.WP.ViewModels.MainPage
{
    public class PagedNewsItems : IPagedNews
    {
        readonly NewsItemGroup vm;
        readonly Dictionary<int, Task<NewsList>> newsListHash;

        public event EventHandler CountChanged;

        public int PageSize { get; private set; }
        public int NumberOfPagesToTakeAtATime { get; private set; }
        public int PageCount { get; private set; }
        public int TotalNewsCount { get; private set; }
        public int NewNewsCount { get; private set; }

        public PagedNewsItems(NewsItemGroup vm, int pageSize, int numberOfPagesToTakeAtATime)
        {
            this.vm = vm;
            this.newsListHash = new Dictionary<int, Task<NewsList>>();
            PageSize = pageSize;
            NumberOfPagesToTakeAtATime = numberOfPagesToTakeAtATime;
            PageCount = 1;
        }

        public IEnumerable<AsyncNewsList> GetNewsLists(EntryType initialEntryType)
        {
            int i = 0;

            while (true)
            {
                yield return GetPage(i, initialEntryType);
                i++;
            }
        }

        AsyncNewsList GetPage(int desiredPage, EntryType initialEntryType)
        {
            var chunkIndex = desiredPage / NumberOfPagesToTakeAtATime;
            var chunkMod = desiredPage % NumberOfPagesToTakeAtATime;

            var newsList = GetNewsListFromHash(chunkIndex, initialEntryType);
            return new AsyncNewsList 
            { 
                News = () => SafelyGetNewsList(newsList, chunkMod)
            };
        }

        Task<NewsList> GetNewsListFromHash(int chunkIndex, EntryType initialEntryType)
        {
            Task<NewsList> newsList;

            if (!newsListHash.TryGetValue(chunkIndex, out newsList))
            {
                if (chunkIndex == 0)
                {
                    newsList = GetNewsListFromVMService(initialEntryType, null, PageSize * NumberOfPagesToTakeAtATime);
                }
                else
                {
                    newsList = GetNewsListRecursive(chunkIndex, initialEntryType);
                }
                newsListHash.Add(chunkIndex, newsList);
            }

            return newsList;
        }

        async Task<NewsList> GetNewsListRecursive(int chunkIndex, EntryType initialEntryType)
        {
            var prevResult = await GetNewsListFromHash(chunkIndex - 1, initialEntryType);
            if (prevResult != null && prevResult.News != null)
            {
                var last = prevResult.News.LastOrDefault();
                if (last != null)
                {
                    var newResult = await GetNewsListFromVMService(EntryType.Peek, last.Id, PageSize * NumberOfPagesToTakeAtATime);
                    return newResult;
                }
            }
            return null;
        }
  
        async Task<NewsList> GetNewsListFromVMService(EntryType entryType, Guid? cursorId, int take)
        {
            var currentNewsList = await vm.GetNewsList(entryType, cursorId, take);

            if (currentNewsList != null)
            {
                PageCount = currentNewsList.GetPageCount(PageSize);
                TotalNewsCount = currentNewsList.TotalArticleCount;
                NewNewsCount = currentNewsList.NewArticleCount;

                if (CountChanged != null)
                    CountChanged(this, EventArgs.Empty);
            }

            return currentNewsList;
        }

        async Task<List<NewsItem>> SafelyGetNewsList(Task<NewsList> load, int skipMult)
        {
            try
            {
                var newsList = await load;
                return newsList.News.Skip(skipMult * PageSize).Take(PageSize).ToList();
            }
            catch
            {
                return new List<NewsItem>();
            }
        }
    }
}



//public IEnumerable<AsyncNewsList> GetNewsLists(EntryType initialEntryType)
//{
//    Lazy<Task<NewsList>> previousPage = null;
//    Lazy<Task<NewsList>> currentPage = null;

//    while (true)
//    {
//        var entryType = i == 0 ? initialEntryType : EntryType.Peek;

//        var takeAmount = PageSize * NumberOfPagesToTakeAtATime;

//        if (previousPage == null)
//        {
//            currentPage = Lazy.Create(() => GetNewsList(entryType, null, takeAmount));
//        }
//        else
//        {
//            currentPage = Lazy.Create(async () =>
//            {
//                var list = await previousPage.Value;
//                if (list.News != null)
//                {
//                    var lastNewsItem = list.News.LastOrDefault();
//                    if (lastNewsItem != null)
//                    {
//                        var cursorId = lastNewsItem.Id;
//                        return await GetNewsList(entryType, cursorId, takeAmount);
//                    }
//                }
//                return new NewsList();
//            });
//        }

//        for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
//        {
//            var skipMult = j;
//            var pageCopy = currentPage;
//            yield return new AsyncNewsList
//            {
//                News = () => SafelyGetNewsList(pageCopy, skipMult),
//            };
//        }

//        previousPage = currentPage;

//        i += NumberOfPagesToTakeAtATime;
//    }
//}

//async Task<NewsList> GetChunk(EntryType entryType, int take)
//{
//    var currentNewsList = await vm.GetNewsList(entryType, cursorId, take);

//    if (currentNewsList != null)
//    {
//        if (currentNewsList.News != null)
//        {
//            var lastNewsItem = currentNewsList.News.LastOrDefault();
//            if (lastNewsItem != null)
//                cursorId = lastNewsItem.Id;
//        }
//        PageCount = currentNewsList.GetPageCount(PageSize);
//        TotalNewsCount = currentNewsList.TotalArticleCount;
//        NewNewsCount = currentNewsList.NewArticleCount;

//        if (CountChanged != null)
//            CountChanged(this, EventArgs.Empty);
//    }

//    return currentNewsList;
//}

//public IEnumerable<AsyncNewsList> GetNewsLists(EntryType initialEntryType)
//{
//    int i = 0;

//    while (true)
//    {
//        var entryType = i == 0 ? initialEntryType : EntryType.Peek;

//        var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
//        //var skipAmount = i * PageSize;
//        var load = Lazy.Create(() => GetChunk(entryType, takeAmount));

//        for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
//        {
//            var skipMult = j;
//            yield return new AsyncNewsList
//            {
//                News = () => SafelyGetNewsList(load, skipMult),
//            };
//        }

//        i += NumberOfPagesToTakeAtATime;
//    }
//}

//async Task<List<NewsItem>> SafelyGetNewsList(Lazy<Task<NewsList>> load, int skipMult)
//{
//    try
//    {
//        var newsList = await load.Value;
//        return newsList.News.Skip(skipMult * PageSize).Take(PageSize).ToList();
//    }
//    catch
//    {
//        return new List<NewsItem>();
//    }
//}