using SelesGames.IsoStorage;
using SelesGames.IsoStorage.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace weave.Data
{
    public class Weave4DataAccessLayer
    {
        public static int MaxAllowedSources = 100;

        readonly string[] FEEDS_KEYS = new string[] { "xFEEDS1", "xFEEDS2", "xFEEDS3" };
        readonly string NEWS_KEY = "xNEWS";

        NLevelRolloverIsoStorageClient<List<FeedSource>> feedsIsoStorageClient;
        IsoStorageClient<List<NewsItem>> newsIsoStorageClient;

        public Lazy<Task<List<FeedSource>>> Feeds { get; private set; }
        public bool IsFirstTime { get; set; }
        public TimeSpan OldMarkedReadNewsElapsedThreshold { get; set; }
        public TimeSpan OldUnreadNewsElapsedThreshold { get; set; }

        public Weave4DataAccessLayer()
        {
            feedsIsoStorageClient = new NLevelRolloverIsoStorageClient<List<FeedSource>>(new ProtobufIsoStorageClient<List<FeedSource>>(), FEEDS_KEYS)
                {
                    TempFileName = "sgwpisNLRC_temp"
                };
            newsIsoStorageClient = new ProtobufIsoStorageClient<List<NewsItem>>();
            
            Feeds = Lazy.Create(GetFeedsAndNewsAsync);
            OldMarkedReadNewsElapsedThreshold = TimeSpan.FromDays(4);
            OldUnreadNewsElapsedThreshold = TimeSpan.FromDays(1);
        }




        #region Private helpers for getting Feeds and News from Iso Storage

        async Task<List<FeedSource>> GetFeedsAndNewsAsync()
        {
            if (IsFirstTime)
                return new List<FeedSource>();

#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            List<FeedSource> feeds = null;
            List<NewsItem> news = null;

            //var feedsTask = GetFeedsAsync();
            //var newsTask = GetNewsAsync();

            try
            {
                //await Task.WhenAll(feedsTask, newsTask);
                //feeds = feedsTask.Result;
                //news = newsTask.Result;
                feeds = await GetFeedsAsync();
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
                    //feed.Initialize(feedsNews, OldMarkedReadNewsElapsedThreshold, OldUnreadNewsElapsedThreshold);
                }
            }

#if DEBUG
            sw.Stop();
            DebugEx.WriteLine("took {0} ms to load data", sw.ElapsedMilliseconds);
#endif

            return feeds;
        }

        public async Task<List<FeedSource>> GetFeedsAsync()
        {
            try
            {
                var feeds = await feedsIsoStorageClient.GetAsync(CancellationToken.None).ConfigureAwait(false);
                return feeds;
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Unable to load feeds from isoStorage", ex) { DataAccessType = "Load: Feeds" };
            }
        }

        //async Task<List<NewsItem>> GetNewsAsync()
        //{
        //    try
        //    {
        //        var news = await newsIsoStorageClient.GetAsync(NEWS_KEY, CancellationToken.None).ConfigureAwait(false);
        //        return news;
        //    }
        //    catch
        //    {
        //        return new List<NewsItem>();
        //    }
        //}

        #endregion




        #region Public access methods




        #region Add Feed

        //public async Task<bool> AddCustomFeed(FeedSource newFeed)
        //{
        //    List<FeedSource> feeds = null;
        //    try
        //    {
        //        feeds = await Feeds.Get().ConfigureAwait(false);
        //    }
        //    catch { }
        //    if (feeds == null)
        //    {
        //        Feeds = Lazy.Create(() => Task.Run(() => new List<FeedSource>()));
        //        feeds = await Feeds.Get().ConfigureAwait(false);
        //    }
        //    if (feeds.Any(feed => feed.Category == newFeed.Category && feed.FeedUri == newFeed.FeedUri))
        //        return false;

        //    feeds.Add(newFeed);
        //    newFeed.Id = Guid.NewGuid();
        //    return true;
        //}

        #endregion




        #region Delete Feed

        //public async Task DeleteFeed(FeedSource feed)
        //{
        //    var feeds = await Feeds.Get().ConfigureAwait(false);
        //    feeds.Remove(feed);
        //}

        #endregion




        #region Save Feeds to IsoStorage

        //public async Task SaveFeeds()
        //{
        //    var feeds = await Feeds.Get().ConfigureAwait(false);
        //    await feedsIsoStorageClient.SaveAsync(feeds, CancellationToken.None).ConfigureAwait(false);
        //}

        #endregion




        #region Save Feeds and News on exit

//        public async Task SaveOnExit()
//        {
//            var feeds = await Feeds.Get().ConfigureAwait(false);
//            var enabledFeeds = feeds.ToList();
//            var news = enabledFeeds.AllNews().ToList();

//            await feedsIsoStorageClient.SaveAsync(enabledFeeds, CancellationToken.None).ConfigureAwait(false);

//#if DEBUG
//            var sw = System.Diagnostics.Stopwatch.StartNew();
//            await newsIsoStorageClient.SaveAsync(NEWS_KEY, news, CancellationToken.None).ConfigureAwait(false);
//            sw.Stop();
//            DebugEx.WriteLine("###################  Took {0} ms to write news to disk", sw.ElapsedMilliseconds);
//#else
//            await newsIsoStorageClient.SaveAsync(NEWS_KEY, news, CancellationToken.None);
//#endif
//        }

        #endregion




        #endregion




        public class DataAccessException : Exception
        {
            public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
            public string DataAccessType { get; set; }
        }
    }
}