using System;
using System.Collections.Generic;
using System.Linq;
using weave.Services.RSS.Cloud;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.Core.DTOs.Incoming;

namespace weave.Services.RSS
{
    public class FeedUpdate
    {
        public bool IsUnchanged { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; set; }
        public IEnumerable<NewsItem> News { get; set; }
    }

    public class NewsServer
    {
        Subject<shim> queueAdder = new Subject<shim>();
        TimeSpan bufferTime = TimeSpan.FromMilliseconds(100);
        //AsyncSubject<bool> useWeaveRssServer = new AsyncSubject<bool>();

        struct shim
        {
            public FeedSource Feed { get; set; }
            public IObserver<FeedUpdate> Observer { get; set; }
        }

        public NewsServer()
        {
            ListenToQueue();
            //WeaveRssServerProxy.PingServerAsync().Subscribe(
           //     o => useWeaveRssServer.OnNext())
        }

        public IObservable<FeedUpdate> GetFeedUpdateAsync(FeedSource feed)
        {
            return Observable.Create<FeedUpdate>(observer =>
            {
                AddToQueue(feed, observer);
                return () => { ; };
            });
        }

        void AddToQueue(FeedSource feed, IObserver<FeedUpdate> observer)
        {
            var x = new shim { Feed = feed, Observer = observer };
            queueAdder.OnNext(x);
        }

        void ListenToQueue()
        {
            queueAdder.BufferUntilTime(bufferTime).Subscribe(ProcessQueue);
            //var timer = Observable.Timer(bufferTime);
            //var input = queueAdder;

            //var query = input.Publish(_input =>
            //    from window in _input.Window(() => _input.Take(1).SelectMany(_ => timer))
            //    from list in window.ToList()
            //    select list);

            //query
                //.Where(o => o.Count > 0)
            //    .Subscribe(ProcessQueue);

            //queueAdder
            //    .WindowWithTime(bufferTime)
            //    //.BufferWithTime(bufferTime)
            //    .Where(o => o.Count > 0)
            //    .Subscribe(ProcessQueue);

            //queueAdder.OnNext(new shim());
        }

        const int idealNewsItemsPerRequest = 20;
        double maxExpectedNewsItemsPerRequest = 20d;

        IEnumerable<IList<shim>> ChunkWork(IList<shim> list)
        {
            var totalExpectedNumberOfNewsItems = 0d;
            var now = DateTime.UtcNow;

            foreach (var shim in list)
            {
                totalExpectedNumberOfNewsItems += getExpectedNumberOfNewsItems(shim, now);
            }

            var split = (int)Math.Floor(totalExpectedNumberOfNewsItems / idealNewsItemsPerRequest);

            if (split == 0d)
                return new List<IList<shim>> { list };

            var take = (int)list.Count / split;

            var temp = new List<IList<shim>>();

            int i;
            for (i = 0; i < split - 1; i++)
            {
                var x = list.Skip(i * take).Take(take).ToList();
                if (x.Count > 0)
                    temp.Add(x);
            }
            var y = list.Skip(i * take).ToList();
            if (y.Count > 0)
                temp.Add(y);

            return temp;
        }

        const double day0Weight = 0.3333;
        const double day1Weight = 0.1;
        const double day2Weight = 0.03333;

        double getExpectedNumberOfNewsItems(shim shim, DateTime now)
        {
            if (shim.Feed == null)
                return 0d;

            var feed = shim.Feed;
            var elapsed = now - feed.LastRefreshedOn;
            var totalHours = elapsed.TotalHours;
            
            var day0 = Math.Min(totalHours, 24d);
            var day1 = Math.Max(0d, Math.Min(totalHours, 48d) - 24d);
            var day2 = Math.Max(0d, totalHours - 48d);

            var expectedNumberOfNewsItems =
                day0 * day0Weight +
                day1 * day1Weight +
                day2 * day2Weight;

            expectedNumberOfNewsItems = Math.Min(maxExpectedNewsItemsPerRequest, expectedNumberOfNewsItems); // cap it off at expecting no more than 30 news items

            return expectedNumberOfNewsItems;
        }

        void ProcessQueue(IList<shim> list)
        {
            if (list == null || list.Count == 0)
                return;

            WeaveRssServerProxy.PingServerAsync().Subscribe(
                useWeave =>
                {
                    if (useWeave)
                        ProcessQueueViaWeaveServer(list);
                    else
                        ProcessQueueViaTraditionalRss(list);
                },
                exception => ProcessQueueViaTraditionalRss(list));
        }

        void ProcessQueueViaWeaveServer(IList<shim> list)
        {
            var chunked = ChunkWork(list);

            foreach (var chunk in chunked)
            {
                var shimLookup = chunk
                    .Select((feed, i) => new { feed, i })
                    .ToDictionary(o => o.i.ToString(), o => o.feed);

                var outgoingFeedRequests = shimLookup.Select(o =>
                    new FeedRequest
                    {
                        Id = o.Key,
                        Url = o.Value.Feed.FeedUri,
                        Etag = o.Value.Feed.Etag,
                        LastModified = o.Value.Feed.LastModified,
                        MostRecentNewsItemPubDate = o.Value.Feed.MostRecentNewsItemPubDate,
                    })
                    .ToList();

                var sw = System.Diagnostics.Stopwatch.StartNew();

                WeaveRssServerProxy.GetFeedResultsAsync(outgoingFeedRequests)
                    .Subscribe(feedResults =>
                    {
                        sw.Stop();
                        //feedResults = feedResults.OrderBy(o => int.Parse(o.Id)).ToList();

                        //DebugEx.WriteLine(outgoingFeedRequests.ToString());
                        DebugEx.WriteLine("Weave server RT time: {0} seconds", sw.Elapsed.TotalSeconds);

                        foreach (var result in feedResults)
                        {
                            var shim = shimLookup[result.Id];
                            var feed = shim.Feed;

                            if (result.Status == FeedResultStatus.OK)
                            {
                                ProcessSuccessfulFeedResult(result, shim);
                            }
                            else if (result.Status == FeedResultStatus.Unmodified)
                            {
                                ProcessUnmodifiedFeedResult(shim);
                            }
                            else
                            {
                                // do backup i.e. original version of calling feed here
                                UpdateFeedTheOldWay(shim);
                            }
                        }
                    },
                    exception =>
                    {
                        foreach (var shim in chunk)
                        {
                            UpdateFeedTheOldWay(shim);
                        }
                    });
            }
        }

        void ProcessQueueViaTraditionalRss(IList<shim> list)
        {
            foreach (var shim in list)
                UpdateFeedTheOldWay(shim);
        }



        #region Process a successful Weave Server feed result

        void ProcessSuccessfulFeedResult(FeedResult result, shim shim)
        {
            try
            {
                var feed = shim.Feed;

                var feedUpdate = new FeedUpdate
                {
                    Etag = result.Etag,
                    LastModified = result.LastModified,
                    MostRecentNewsItemPubDate = result.MostRecentNewsItemPubDate,
                    OldestNewsItemPubDate = result.OldestNewsItemPubDate,
                    News = result.News != null ? result.News
                        .Select(o => new { newsItem = o, pubDateTry = RssServiceLayer.TryGetLocalDate(o.PublishDateTime) })
                        .Where(o => o.pubDateTry.Item1)
                        //.Select(o => o.newsItem)
                        .Select(o =>
                            new NewsItem
                            {
                                Title = o.newsItem.Title,
                                PublishDateTime = o.pubDateTry.Item2,
                                Description = o.newsItem.Description,
                                ImageUrl = o.newsItem.ImageUrl,
                                Link = o.newsItem.Link,
                                HasBeenViewed = false,
                                FeedSource = feed,
                            })
                        : new List<NewsItem>(0),
                };
                shim.Observer.OnNext(feedUpdate);
                shim.Observer.OnCompleted();
            }
            catch (Exception)
            {
                UpdateFeedTheOldWay(shim);
            }
        }

        #endregion



        #region Process an unmodified Weave Server feed result

        void ProcessUnmodifiedFeedResult(shim shim)
        {
            shim.Observer.OnNext(new FeedUpdate { IsUnchanged = true });
            shim.Observer.OnCompleted();
        }

        #endregion



        #region Call the feed's url directly

        void UpdateFeedTheOldWay(shim shim)
        {
            var feed = shim.Feed;

            var feedRequester = new FeedRequester
            {
                Etag = feed.Etag,
                FeedUri = feed.FeedUri,
                MostRecentNewsItemPubDate = feed.MostRecentNewsItemPubDate,
            };

            feedRequester.UpdateFeed().Subscribe(requestStatus =>
            {
                try
                {
                    if (requestStatus == FeedRequester.RequestStatus.OK)
                    {
                        var news = feedRequester.News;
                        foreach (var newsItem in news)
                            newsItem.FeedSource = feed;

                        var feedUpdate = new FeedUpdate
                        {
                            Etag = feedRequester.Etag,
                            LastModified = null,//feedRequester.LastModified,
                            MostRecentNewsItemPubDate = feedRequester.MostRecentNewsItemPubDate,
                            OldestNewsItemPubDate = feedRequester.OldestNewsItemPubDate,
                            News = news,
                        };

                        shim.Observer.OnNext(feedUpdate);
                        shim.Observer.OnCompleted();
                    }
                }
                catch (Exception exception)
                {
                    shim.Observer.OnError(exception);
                }
            },

            shim.Observer.OnError);
        }

        #endregion
    }
}