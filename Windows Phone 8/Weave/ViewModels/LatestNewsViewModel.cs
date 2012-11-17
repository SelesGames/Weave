using SelesGames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace weave
{
    public class LatestNewsViewModel : INotifyPropertyChanged
    {
        public IEnumerable<NewsItem> LatestNews { get; set; }
        IEnumerable<NewsItem> previousNewsItems = new List<NewsItem>();

        public async Task RefreshNewsAsync()
        {
            var newNewsItems = await GetTopNewsItems();
            bool areItemsNew = !Enumerable.SequenceEqual(newNewsItems, previousNewsItems);

            if (areItemsNew)
            {
                previousNewsItems = newNewsItems;
                LatestNews = newNewsItems;
                GlobalDispatcher.Current.BeginInvoke(() => PropertyChanged.Raise(this, "LatestNews"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;




        #region static helper methods for creating the latest/featured news

        const int pageSize = 8;

        static async Task<IEnumerable<NewsItem>> GetTopNewsItems()
        {
            var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
            var feeds = await dal.Feeds.Get();

            IEnumerable<NewsItem> pool = feeds
                .AllOrderedNews()
                .Where(news => !news.HasBeenViewed)
                .Distinct(NewsItemComparer.Instance)
                .Take(20)
                .ToList();

            pool = CreatePool(pool);
            var topNewsItems = pool.Take(pageSize).ToList();
            return topNewsItems;
        }

        static IEnumerable<NewsItem> CreatePool(IEnumerable<NewsItem> allNewsItems)
        {
            return allNewsItems
                .Select(i => new
                {
                    NewsItem = i,
                    AdjustedSortRating = GetAdjustedForImagePresenceSortRating(i),
                })
                .Select(i =>
                new
                {
                    NewsItem = i.NewsItem,
                    FinalAdjustedSortRating = GetAdjustedForRepetitiveFeedSortRating(i.AdjustedSortRating)
                })
                .OrderByDescending(i => i.FinalAdjustedSortRating)
                .Select(i => i.NewsItem);
        }

        static double GetAdjustedForImagePresenceSortRating(NewsItem i)
        {
            return i.HasImage ? 100d * i.SortRating : i.SortRating;
        }

        static double GetAdjustedForRepetitiveFeedSortRating(double i)
        {
            return i;
        }

        #endregion
    }
}