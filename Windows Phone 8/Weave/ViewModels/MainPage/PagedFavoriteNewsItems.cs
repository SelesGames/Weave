using SelesGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;

namespace weave
{
    public class PagedFavoriteNewsItems : IPagedNewsItems
    {
        IUserCache userCache = ServiceResolver.Get<IUserCache>();
        UserInfo user;
        IEnumerable<AsyncNewsList> newsLists;// = new List<AsyncNewsList>();

        public int PageSize { get; private set; }
        public int NumberOfPagesToTakeAtATime { get; private set; }
        public int PageCount { get; private set; }
        public int TotalNewsCount { get; private set; }
        public int NewNewsCount { get; private set; }

        public PagedFavoriteNewsItems(int pageSize, int numberOfPagesToTakeAtATime)
        {
            PageSize = pageSize;
            NumberOfPagesToTakeAtATime = numberOfPagesToTakeAtATime;

            user = userCache.Get();
        }

        public async Task Refresh(EntryType entry)
        {
            //var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
            //var currentNewsList = await user.GetFavorites(0, takeAmount);

            // don't have any of this info when dealing with Favorites
            PageCount = 999;
            TotalNewsCount = 9999;
            NewNewsCount = 0;

            newsLists = DoTheChunka().Memoize();

            //for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
            //{
            //    var skipMult = j;
            //    var t = new AsyncNewsList
            //    {
            //        News = () => Task.FromResult(currentNewsList.Skip(skipMult * PageSize).Take(PageSize).ToList()),
            //    };

            //    newsLists.Add(t);
            //}

            //Chunk();
        }

        IEnumerable<AsyncNewsList> DoTheChunka()
        {
            int i = 0;

            while (true)
            {
                var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
                var skipAmount = i * PageSize;
                var load = Lazy.Create(() => user.GetFavorites(skipAmount, takeAmount));

                for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
                {
                    var skipMult = j;
                    var t = new AsyncNewsList
                    {
                        News = async () => (await load.Get()).Skip(skipMult * PageSize).Take(PageSize).ToList(),
                    };

                    yield return t;
                }

                i += NumberOfPagesToTakeAtATime;
            }
        }

        //void Chunk()
        //{
        //    for (int i = NumberOfPagesToTakeAtATime; i < PageCount; i += NumberOfPagesToTakeAtATime)
        //    {
        //        var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
        //        var skipAmount = i * PageSize;
        //        var load = Lazy.Create(() => user.GetFavorites(skipAmount, takeAmount));

        //        for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
        //        {
        //            var skipMult = j;
        //            var t = new AsyncNewsList
        //            {
        //                News = async () => (await load.Get()).Skip(skipMult * PageSize).Take(PageSize).ToList(),
        //            };

        //            newsLists.Add(t);
        //        }
        //    }
        //}

        public AsyncNewsList GetNewsFuncForPage(int page)
        {
            //return newsLists[page];
            return newsLists.Skip(page).First();
        }
    }
}
