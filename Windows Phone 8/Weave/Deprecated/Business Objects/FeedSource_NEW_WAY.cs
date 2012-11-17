//using System;
//using System.Collections.Generic;
//using System.Linq;
//using ProtoBuf;
//using weave.Services.RSS;

//namespace weave
//{
//    [ProtoContract] 
//    public class FeedSource
//    {
//        [ProtoMember(1)] public string Category { get; set; }
//        [ProtoMember(2)] public string FeedUri { get; set; }
//        [ProtoMember(3)] public string FeedName { get; set; }
//        [ProtoMember(4)] public bool IsEnabled { get; set; }
//        [ProtoMember(5)] public bool UserAdded { get; set; }
//        [ProtoMember(6)] public string Etag { get; set; }
//        [ProtoMember(7)] public DateTime LastRefreshedOn { get; set; }  // NEVER USED
//        [ProtoMember(8)] public List<NewsItem> News { get; set; }
//        [ProtoMember(9)] public bool IsUserAdded { get; set; }

//        [ProtoMember(10)] public string MostRecentNewsItemPubDate { get; set; }
//        [ProtoMember(11)] public string OldestNewsItemPubDate { get; set; }
//        [ProtoMember(12)] public string LastModified { get; set; }

//        static NewsItemComparer newsItemComparer = new NewsItemComparer();

//        //bool isOldNewsLoadedStarted;
//        //bool isOldNewsLoadedCompleted;

//        object syncObject = new object();

//        public FeedSource()
//        {
//            News = new List<NewsItem>();
//        }

//        Subject<Unit> newsWasRefreshedNotificationStream = new Subject<Unit>();
//        public IObservable<Unit> NewsWasRefreshedNotificationStream { get { return newsWasRefreshedNotificationStream.AsObservable(); } }
        
//        public void Reset()
//        {
//            Etag = null;
//            LastModified = null;
//            MostRecentNewsItemPubDate = null;
//            OldestNewsItemPubDate = null;
//            LastRefreshedOn = DateTime.MinValue;
//            lock(syncObject)
//                News = null;
//        }

//        public override string ToString()
//        {
//            return string.Format("{0} - {1} - {2}", Category, FeedName, FeedUri);
//        }

//        public void CheckForAndGetNewFeedData()
//        {
//            var newsServer = ServiceResolver.Get<NewsServer>();
//            var updatedRequest = new FeedUpdateRequest
//            {
//                Etag = this.Etag,
//                FeedUri = this.FeedUri,
//                LastModified = this.LastModified,
//                LastRefreshedOn = this.LastRefreshedOn,
//                MostRecentNewsItemPubDate = this.MostRecentNewsItemPubDate,
//            };
//            newsServer.GetFeedUpdateAsync(updatedRequest)
//                .Subscribe(
//                    HandleUpdate, 
//                    HandleException);
//        }

//        void HandleUpdate(FeedUpdateResponse update)
//        {
//            this.LastRefreshedOn = DateTime.UtcNow;

//            if (update.IsUnchanged)
//                return;

//            //if (isOldNewsLoadedCompleted)
//                DeleteNewsOlderThan(update.OldestNewsItemPubDate);

//            AddNews(update.News);
//            this.Etag = update.Etag;
//            this.LastModified = update.LastModified;
//            this.MostRecentNewsItemPubDate = update.MostRecentNewsItemPubDate;
//            this.OldestNewsItemPubDate = OldestNewsItemPubDate;
//        }

//        void DeleteNewsOlderThan(string date)
//        {
//            var tryGetOldestDate = weave.Services.RSS.RssServiceLayer.TryGetLocalDate(date);
//            if (tryGetOldestDate.Item1)
//            {
//                var oldestPubDate = tryGetOldestDate.Item2;
//                lock (syncObject)
//                {
//                    var correctedNews = News.TakeWhile(o => o.PublishDateTime >= oldestPubDate).ToList();
//                    News = correctedNews;
//                }   
//            }
//        }

//        void AddNews(IEnumerable<weave.Services.RSS.NewsItem> newNews)
//        {
//            if (newNews == null || !newNews.Any())
//                return;

//            var trulyNewsNews = new List<NewsItem>();
//            foreach (var o in newNews)
//            {
//                if (!DoesAnyExistingNewsItemMatch(o))
//                    trulyNewsNews.Add(AsNewsItem(o));
//            }

//            if (trulyNewsNews.Count > 0)
//            {
//                foreach (var newsItem in trulyNewsNews)
//                    newsItem.IsNew = true;

//                lock (syncObject)
//                {
//                    News.InsertRange(0, trulyNewsNews);
//                    newsWasRefreshedNotificationStream.OnNext(new Unit());
//                }
//            }
//        }

//        bool DoesAnyExistingNewsItemMatch(weave.Services.RSS.NewsItem newNewsItem)
//        {
//            if (News == null || News.Count == 0)
//                return false;

//            return News.Any(newsItem => newsItem.Title.Equals(newNewsItem.Title));
//        }

//        NewsItem AsNewsItem(weave.Services.RSS.NewsItem o)
//        {
//            return new NewsItem
//            {
//                Title = o.Title,
//                Link = o.Link,
//                Description = o.Description,
//                ImageUrl = o.ImageUrl,
//                PublishDateTime = o.PublishDateTime,
//                HasBeenViewed = false,
//                IsNew = true,
//                FeedSource = this,
//            };
//        }

//        void HandleException(Exception exception)
//        {
//        }


//        //public void LoadOldNewsItems()
//        //{
//        //    if (isOldNewsLoadedStarted)
//        //        return;

//        //    isOldNewsLoadedStarted = true;

//        //    var oldNews = this.GetAllOldNewsItems();
//        //    if (oldNews != null)
//        //    {
//        //        foreach (var newsItem in oldNews)
//        //            newsItem.FeedSource = this;

//        //        lock (syncObject)
//        //        {
//        //            News.AddRange(oldNews);
//        //            newsWasRefreshedNotificationStream.OnNext(new Unit());
//        //        }
//        //    }

//        //    isOldNewsLoadedCompleted = true;
//        //}

//        //public void SaveNews()
//        //{
//        //    this.SaveNewsItems(News, isOldNewsLoadedCompleted);
//        //}

//        class NewsItemComparer : IEqualityComparer<NewsItem>
//        {
//            public bool Equals(NewsItem x, NewsItem y)
//            {
//                return x.Title.Equals(y.Title);
//            }

//            public int GetHashCode(NewsItem obj)
//            {
//                return obj.Title.GetHashCode();
//            }

//            bool RelaxedDateTimeEquality(DateTime x, DateTime y) // if it is 10 minutes diff or less, treat as equal
//            {
//                return Math.Abs((x - y).TotalHours) <= 48d;
//            }
//        }
//    }
//}
