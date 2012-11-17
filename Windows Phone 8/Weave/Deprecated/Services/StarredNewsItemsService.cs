using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace weave
{
    public static class StarredNewsItemsService
    {
        static object syncObject = new object();
        static List<StarredNewsItem> starredNewsItems;
        static IObservable<List<StarredNewsItem>> starredNewsItemsStream;

        static StarredNewsItemsService()
        {
            starredNewsItems = new List<StarredNewsItem>();
        }

        internal static void Initialize()
        {
            starredNewsItemsStream = Observable.Start(() => IsolatedStorageService.GetStarredNewsItems());
            starredNewsItemsStream.Subscribe(o =>
            {
                if (o == null)
                    return;

                lock (syncObject)
                {
                    starredNewsItems = o.Union(starredNewsItems).ToList();
                }
                foreach (var newsItem in starredNewsItems)
                    newsItem.CheckIfFullArticleIsPresent();
            });
        }

        internal static StarredNewsItem GetRandomFeaturedStarredNewsItem()
        {
            return starredNewsItems.RandomlySort().FirstOrDefault();
        }

        internal static void AddToStarredItems(NewsItem newsItem)
        {
            if (IsStarred(newsItem))
                return;

            var starredItem = newsItem.ToStarredNewsItem();
            starredItem.CheckIfFullArticleIsPresent();
            starredNewsItems.Add(starredItem);
        }

        internal static bool IsStarred(NewsItem newsItem)
        {
            if (newsItem == null)
                return false;

            return starredNewsItems.Where(o =>
                o.Link == newsItem.Link).Count() > 0;
        }

        internal static IObservable<IEnumerable<StarredNewsItem>> StarredNewsItemsStream
        {
            get
            {
                lock (syncObject)
                {
                    if (starredNewsItems == null)
                        return starredNewsItemsStream.Select(o => o.AsEnumerable());
                    else
                        return new[] { starredNewsItems.AsEnumerable() }.ToObservable();
                }
            }
        }

        internal static void Save()
        {
            IsolatedStorageService.SaveStarredNewsItems(starredNewsItems);
        }
    }
}
