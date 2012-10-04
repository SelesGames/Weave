using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SelesGames.WP.IsoStorage;
using SelesGames.WP.IsoStorage.Protobuf;
using weave;
using Stored = Weave.Data.Storage.DTOs;

namespace Weave.Data.Storage
{
    public class DataClient
    {
        public static int MaxAllowedSources = 60;

        readonly string[] FEEDS_KEYS = new string[] { "xFEEDS1", "xFEEDS2", "xFEEDS3" };
        readonly string NEWS_KEY = "xNEWS";

        NLevelRolloverIsoStorageClient<List<Stored.FeedSource>> feedsIsoStorageClient;
        IsoStorageClient<List<Stored.NewsItem>> newsIsoStorageClient;

        public Lazy<Task<List<FeedSource>>> Feeds { get; private set; }
        public bool IsFirstTime { get; set; }
        public TimeSpan OldMarkedReadNewsElapsedThreshold { get; set; }
        public TimeSpan OldUnreadNewsElapsedThreshold { get; set; }

        public DataClient()
        {
            feedsIsoStorageClient = new NLevelRolloverIsoStorageClient<List<Stored.FeedSource>>(new ProtobufIsoStorageClient<List<Stored.FeedSource>>(), FEEDS_KEYS)
                {
                    TempFileName = "sgwpisNLRC_temp"
                };
            newsIsoStorageClient = new ProtobufIsoStorageClient<List<Stored.NewsItem>>();
            
            Feeds = Lazy.Create(GetFeedsAndNewsAsync);
            OldMarkedReadNewsElapsedThreshold = TimeSpan.FromDays(4);
            OldUnreadNewsElapsedThreshold = TimeSpan.FromDays(1);
        }



        #region core function for getting data from storage, performing necessary operations, and returning the proper classes

        async Task<List<FeedSource>> GetFeedsAndNewsAsync()
        {
            if (IsFirstTime)
                return new List<FeedSource>();

#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            List<FeedSource> feeds = null;
            List<NewsItem> news = null;

            var feedsTask = GetFeedsAsync();
            var newsTask = GetNewsAsync();

            try
            {
                await TaskEx.WhenAll(feedsTask, newsTask);
                feeds = feedsTask.Result;
                news = newsTask.Result;
            }
            catch (Exception ex)
            {
                if (ex is DataAccessException)
                    throw ex;
            }

            if (news != null)
            {
                foreach (var feed in feeds)
                {
                    // get the news for this particular feed
                    var feedsNews = news.Where(o => o.FeedId == feed.Id).ToList();
                    feed.Initialize(feedsNews, OldMarkedReadNewsElapsedThreshold, OldUnreadNewsElapsedThreshold);
                }
            }

#if DEBUG
            sw.Stop();
            DebugEx.WriteLine("took {0} ms to load data", sw.ElapsedMilliseconds);
#endif

            return feeds;
        }

        #endregion




        #region Private helpers for getting Feeds and News from Iso Storage then converting them to the proper Data Objects

        async Task<List<FeedSource>> GetFeedsAsync()
        {
            try
            {
                var storedFeeds = await feedsIsoStorageClient.GetAsync(CancellationToken.None);
                var feeds = storedFeeds.Select(Parse).ToList();
                return feeds;
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Unable to load feeds from isoStorage", ex) { DataAccessType = "Load: Feeds" };
            }
        }

        async Task<List<NewsItem>> GetNewsAsync()
        {
            try
            {
                var storedNews = await newsIsoStorageClient.GetAsync(NEWS_KEY, CancellationToken.None);
                var news = storedNews.Select(Parse).ToList();
                return news;
            }
            catch
            {
                return new List<NewsItem>();
            }
        }

        static FeedSource Parse(Stored.FeedSource source)
        {
            return new FeedSource
            {
                Id = source.Id,
                FeedName = source.FeedName,
                FeedUri = source.FeedUri,
                Category = source.Category,
                Etag = source.Etag,
                LastModified = source.LastModified,
                MostRecentNewsItemPubDate = source.MostRecentNewsItemPubDate,
                LastRefreshedOn = source.LastRefreshedOn,
                NewsHash = source.NewsHash,
                ArticleViewingType = source.ArticleViewingType,
                UpdateHistory = source.UpdateHistory != null ? source.UpdateHistory.Select(Parse).ToList() : null,
            };
        }

        static NewsItem Parse(Stored.NewsItem o)
        {
            return new NewsItem
            {
                FeedId = o.FeedId,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                PublishDateTime = o.PublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = o.IsFavorite,
                HasBeenViewed = o.HasBeenViewed,
            };
        }

        static UpdateParameters Parse(Stored.UpdateParameters o)
        {
            return new UpdateParameters
            {
                Etag = o.Etag,
                LastModified = o.LastModified,
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                NewsHash = o.NewsHash,
            };
        }

        #endregion




        #region Public access methods




        #region Add Feed

        public async Task<bool> AddCustomFeed(FeedSource newFeed)
        {
            List<FeedSource> feeds = null;
            try
            {
                feeds = await Feeds.Get().ConfigureAwait(false);
            }
            catch { }
            if (feeds == null)
            {
                Feeds = Lazy.Create(() => TaskEx.Run(() => new List<FeedSource>()));
                feeds = await Feeds.Get().ConfigureAwait(false);
            }
            if (feeds.Any(feed => feed.Category == newFeed.Category && feed.FeedUri == newFeed.FeedUri))
                return false;

            feeds.Add(newFeed);
            newFeed.Id = Guid.NewGuid();
            return true;
        }

        #endregion




        #region Delete Feed

        public async Task DeleteFeed(FeedSource feed)
        {
            var feeds = await Feeds.Get();
            feeds.Remove(feed);
        }

        #endregion




        #region Save Feeds to IsoStorage

        public async Task SaveFeeds()
        {
            var feeds = await Feeds.Get().ConfigureAwait(false);
            await feedsIsoStorageClient.SaveAsync(feeds, CancellationToken.None);
        }

        #endregion




        #region Save Feeds and News on exit

        public async Task SaveOnExit()
        {
            var feeds = await Feeds.Get();
            var enabledFeeds = feeds.ToList();
            var news = enabledFeeds.AllNews().ToList();

            await feedsIsoStorageClient.SaveAsync(enabledFeeds, CancellationToken.None);

#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await newsIsoStorageClient.SaveAsync(NEWS_KEY, news, CancellationToken.None);
            sw.Stop();
            DebugEx.WriteLine("###################  Took {0} ms to write news to disk", sw.ElapsedMilliseconds);
#else
            await newsIsoStorageClient.SaveAsync(NEWS_KEY, news, CancellationToken.None);
#endif
        }

        #endregion




        #endregion




        public class DataAccessException : Exception
        {
            public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
            public string DataAccessType { get; set; }
        }
    }
}