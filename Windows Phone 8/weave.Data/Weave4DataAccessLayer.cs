using SelesGames.IsoStorage;
using SelesGames.IsoStorage.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Weave.Data
{
    public class Weave4DataAccessLayer
    {
        public static int MaxAllowedSources = 100;

        readonly string[] FEEDS_KEYS = new string[] { "xFEEDS1", "xFEEDS2", "xFEEDS3" };
        readonly string NEWS_KEY = "xNEWS";

        NLevelRolloverIsoStorageClient<List<FeedSource>> feedsIsoStorageClient;
        IsoStorageClient<List<NewsItem>> newsIsoStorageClient;

        public Lazy<Task<List<FeedSource>>> Feeds { get; private set; }

        public Weave4DataAccessLayer()
        {
            feedsIsoStorageClient = new NLevelRolloverIsoStorageClient<List<FeedSource>>(new ProtobufIsoStorageClient<List<FeedSource>>(), FEEDS_KEYS)
                {
                    TempFileName = "sgwpisNLRC_temp"
                };
            newsIsoStorageClient = new ProtobufIsoStorageClient<List<NewsItem>>();
            
            Feeds = Lazy.Create(GetFeedsAndNewsAsync);
        }




        #region Private helpers for getting Feeds and News from Iso Storage

        async Task<List<FeedSource>> GetFeedsAndNewsAsync()
        {
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            List<FeedSource> feeds = null;
            List<NewsItem> news = null;

            var feedsTask = GetFeedsAsync();
            var newsTask = GetNewsAsync();

            try
            {
                await Task.WhenAll(feedsTask, newsTask);
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
                    feed.News = feedsNews;
                    foreach (var newsItem in feedsNews)
                    {
                        newsItem.FeedSource = feed;
                    }
                }
            }

#if DEBUG
            sw.Stop();
            DebugEx.WriteLine("took {0} ms to load data", sw.ElapsedMilliseconds);
#endif

            return feeds;
        }

        async Task<List<FeedSource>> GetFeedsAsync()
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

        async Task<List<NewsItem>> GetNewsAsync()
        {
            try
            {
                var news = await newsIsoStorageClient.GetAsync(NEWS_KEY, CancellationToken.None).ConfigureAwait(false);
                return news;
            }
            catch
            {
                return new List<NewsItem>();
            }
        }

        #endregion




        #region Public access methods

        public void Clear()
        {
        }


        #endregion




        public class DataAccessException : Exception
        {
            public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
            public string DataAccessType { get; set; }
        }
    }
}