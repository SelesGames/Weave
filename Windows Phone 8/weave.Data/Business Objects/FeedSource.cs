using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Weave.RSS;

namespace weave
{
    [ProtoContract] 
    public class FeedSource
    {
        static NewsItemComparer newsItemComparer = new NewsItemComparer();
        static UpdateParametersComparer updateParamsComparer = new UpdateParametersComparer();
        static NewsServer newsServer = new NewsServer();
        public static TimeSpan RefreshThreshold = TimeSpan.FromMinutes(15);

        object syncObject = new object();
        bool isUpdating = false;
        FILOQueueWrapper<UpdateParameters> updateHistoryQueue;


        public static NewsServer NewsServer
        {
            get { return newsServer; }
            set { newsServer = value; }
        }


        [ProtoMember(1)]    public Guid Id { get; set; }
        [ProtoMember(2)]    public string FeedName { get; set; }
        [ProtoMember(3)]    public string FeedUri { get; set; }
        [ProtoMember(4)]    public string Category { get; set; }
        [ProtoMember(5)]    public string Etag { get; set; }
        [ProtoMember(6)]    public string LastModified { get; set; }
        [ProtoMember(7)]    public string MostRecentNewsItemPubDate { get; set; }
        [ProtoMember(8)]    public DateTime LastRefreshedOn { get; set; }
        [ProtoMember(9)]    public Guid NewsHash { get; set; }
        [ProtoMember(10)]   public ArticleViewingType ArticleViewingType { get; set; }
        [ProtoMember(11)]   public List<UpdateParameters> UpdateHistory { get; set; }




        public List<NewsItem> News { get; set; }
        public Task CurrentRefresh { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", Category, FeedName, FeedUri);
        }

        public void RefreshNews()
        {
            if (isUpdating)
                return;

            CurrentRefresh = refreshNews();
        }

        async Task refreshNews()
        {
            if (isUpdating)
                return;

            isUpdating = true;

            if (newsServer == null)
                throw new Exception("no NewsServer was registered via the Service Resolver");

            var updatedRequest = new FeedUpdateRequest
            {
                Etag = Etag,
                FeedUri = FeedUri,
                LastModified = LastModified,
                LastRefreshedOn = LastRefreshedOn,
                MostRecentNewsItemPubDate = MostRecentNewsItemPubDate,
            };
            
            try
            {
                var update = await newsServer.GetFeedUpdateAsync(updatedRequest).ConfigureAwait(false);
                HandleUpdate(update);
            }
            catch(Exception exception)
            {
                DebugEx.WriteLine(exception.Message);
            }
            
            isUpdating = false;
        }




        #region Helper functions for handling an update or update exception

        void HandleUpdate(FeedUpdateResponse update)
        {
            if (!update.IsUnchanged)
            {
                DeleteNewsOlderThan(update.OldestNewsItemPubDate);
                AddNews(update.News);
                RecalculateNewsHash();

                Etag = update.Etag;
                LastModified = update.LastModified;
                MostRecentNewsItemPubDate = update.MostRecentNewsItemPubDate;
                SaveToUpdateHistory();
            }

            LastRefreshedOn = DateTime.UtcNow;
        }

        void DeleteNewsOlderThan(string date)
        {
            if (News == null || !News.Any())
                return;

            var tryGetOldestDate = Weave.RSS.RssDateParsingHelper.TryGetLocalDate(date);
            if (tryGetOldestDate.Item1)
            {
                var oldestPubDate = tryGetOldestDate.Item2;
                lock (syncObject)
                {
                    // keep all news that is newer than the oldest pub date, as well as all favorited news
                    var correctedNews = News.Where(o => o.IsFavorite || o.PublishDateTime >= oldestPubDate).ToList();
                    News = correctedNews;
                }   
            }
        }

        void AddNews(IEnumerable<Weave.RSS.NewsItem> newNews)
        {
            if (newNews == null || !newNews.Any())
                return;

            var newNewsInCorrectFormat = newNews.Where(o => !DoesAnyExistingNewsItemMatch(o)).Select(AsNewsItem).ToList();
            AddNewNewsItems(newNewsInCorrectFormat);
        }

        void AddNewNewsItems(IEnumerable<NewsItem> newsToAdd)
        {
            if (newsToAdd == null || !newsToAdd.Any())
                return;

            var originalDownloadDateTime = DateTime.UtcNow;
            foreach (var newsItem in newsToAdd)
                newsItem.OriginalDownloadDateTime = originalDownloadDateTime;

            lock (syncObject)
            {
                if (News == null)
                    News = new List<NewsItem>();

                News.InsertRange(0, newsToAdd);
                //newsWasRefreshedNotificationStream.OnNext(Unit.Default);
            }
        }

        bool DoesAnyExistingNewsItemMatch(Weave.RSS.NewsItem newNewsItem)
        {
            if (News == null || News.Count == 0)
                return false;

            return News.Any(newsItem => newsItem.Title.Equals(newNewsItem.Title));
        }

        NewsItem AsNewsItem(Weave.RSS.NewsItem o)
        {
            return new NewsItem
            {
                FeedId = this.Id,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                PublishDateTime = o.PublishDateTime,
                HasBeenViewed = false,
                FeedSource = this,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
            };
        }

        #endregion




        public void Initialize(List<NewsItem> feedsNews, TimeSpan markedReadyExpiry, TimeSpan unreadExpiry)
        {
            if (UpdateHistory == null || updateHistoryQueue == null)
                InitializeUpdateHistoryQueue();

            foreach (var newsItem in feedsNews)
                newsItem.FeedSource = this;

            // check to see if the recovered news is valid
            var feedsNewsHash = CalculateHashOfNews(feedsNews);
            var isFeedStateConsistentWithRecoveredNews = feedsNewsHash.Equals(NewsHash);

            if (!isFeedStateConsistentWithRecoveredNews)
            {
                while (!isFeedStateConsistentWithRecoveredNews && updateHistoryQueue.Any())
                {
                    var entry = updateHistoryQueue.Dequeue();
                    RollbackUpdateStateTo(entry);
                    isFeedStateConsistentWithRecoveredNews = feedsNewsHash.Equals(NewsHash);
                }

                if (isFeedStateConsistentWithRecoveredNews)
                    News = feedsNews;
                else
                    ResetUpdateState();
            }
            else
                News = feedsNews;

            DeleteNewsOlderThan(markedReadyExpiry, unreadExpiry);
            RecalculateNewsHash();
            SaveToUpdateHistory();
        }

        void InitializeUpdateHistoryQueue()
        {
            if (UpdateHistory == null)
                UpdateHistory = new List<UpdateParameters>();

            if (updateHistoryQueue == null)
                updateHistoryQueue = new FILOQueueWrapper<UpdateParameters>(UpdateHistory, 10);
        }

        void RecalculateNewsHash()
        {
            NewsHash = CalculateHashOfNews(News);
        }

        void SaveToUpdateHistory()
        {
            if (UpdateHistory == null || updateHistoryQueue == null)
                InitializeUpdateHistoryQueue();

            var entry = new UpdateParameters
            {
                Etag = Etag,
                LastModified = LastModified,
                MostRecentNewsItemPubDate = MostRecentNewsItemPubDate,
                NewsHash = NewsHash
            };
            updateHistoryQueue.QueueUnique(entry, updateParamsComparer);
        }




        #region Helper functions to aid in initialized the News with what was recovered from isostorage

        void RollbackUpdateStateTo(UpdateParameters entry)
        {
            Etag = entry.Etag;
            LastModified = entry.LastModified;
            MostRecentNewsItemPubDate = entry.MostRecentNewsItemPubDate;
            NewsHash = entry.NewsHash;
        }

        void ResetUpdateState()
        {
            Etag = null;
            LastModified = null;
            MostRecentNewsItemPubDate = null;
            LastRefreshedOn = DateTime.MinValue;
            News = null;
        }

        void DeleteNewsOlderThan(TimeSpan markedReadyExpiry, TimeSpan unreadExpiry)
        {
            if (News == null || !News.Any())
                return;

            var now = DateTime.UtcNow;

            lock (syncObject)
            {
                News = News.Where(o => 
                    o.IsFavorite ||
                    (!o.HasBeenViewed && (now - o.OriginalDownloadDateTime) < unreadExpiry)
                    ||
                    (o.HasBeenViewed && (now - o.OriginalDownloadDateTime) < markedReadyExpiry)).ToList();
            }
        }

        #endregion




        #region Helper functions for calculating the hash value of a collection of news links

        static Guid CalculateHashOfNews(IEnumerable<NewsItem> news)
        {
            if (news == null || !news.Any())
                return Guid.Empty;
            else
            {
                var concatenatedNewsLinks = news
                    .OfType<NewsItem>()
                    .Select(o => o.Link)
                    .OfType<string>()
                    .Aggregate(new StringBuilder(), (sb, link) => sb.Append(link))
                    .ToString();
                var guid = ComputeHash(concatenatedNewsLinks);
                return guid;
            }
        }

        // approach using MD5 and GUIDs
        static Guid ComputeHash(string val)
        {
            var md5 = new System.Security.Cryptography.SHA1Managed();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(val);
            byte[] hash = md5.ComputeHash(inputBytes);
            Array.Resize(ref hash, 16);
            var guid = new Guid(hash);
            //string hashString = guid.ToString("N");
            //return hashString;
            return guid;
        }

        #endregion




        #region Overrides of the Equals and GetHashCode functions for determining equality

        public override bool Equals(object obj)
        {
            var that = obj as FeedSource;
            if (that == null || this.FeedUri == null)
                return false;

            return this.FeedUri.Equals(that.FeedUri, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return FeedUri != null ? FeedUri.GetHashCode() : -1;
        }

        #endregion
    }
}