using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RSS.DTOs.Incoming;
using Weave.RSS.DTOs.Outgoing;

namespace Weave.RSS
{
    public class NewsServer
    {
        List<shim> updateQueue = new List<shim>();
        bool isBatching = false;

        public void BeginFeedUpdateBatch()
        {
            isBatching = true;
        }

        public void EndFeedUpdateBatch()
        {
            var copy = updateQueue.ToList();
            updateQueue.Clear();
            ProcessQueueViaWeaveServer(copy);
        }

        struct shim
        {
            public FeedUpdateRequest Feed { get; set; }
            public TaskCompletionSource<FeedUpdateResponse> TaskSource { get; set; }
        }

        public Task<FeedUpdateResponse> GetFeedUpdateAsync(FeedUpdateRequest feed)
        {
            var t = new TaskCompletionSource<FeedUpdateResponse>();

            if (isBatching)
            {
                AddToQueue(feed, t);
                return t.Task;
            }
            else
            {
                ProcessQueueViaWeaveServer(new[] { new shim { Feed = feed, TaskSource = t }});
                return t.Task;
            }
        }

        void AddToQueue(FeedUpdateRequest feed, TaskCompletionSource<FeedUpdateResponse> taskSource)
        {
            var x = new shim { Feed = feed, TaskSource = taskSource };
            updateQueue.Add(x);
        }

        async void ProcessQueueViaWeaveServer(IList<shim> list)
        {
            if (list == null || !list.Any())
                return;

            var chunked = ChunkWork(list);

            foreach (var chunk in chunked)
            {
                var capturedGroup = chunk;

                var shimLookup = capturedGroup
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

                try
                { 
                    var feedResults = await new WeaveRssServerProxy(outgoingFeedRequests).GetFeedResultsAsync().ConfigureAwait(false); 

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
                            shim.TaskSource.TrySetException(new Exception(string.Format("weave server failed to download {0}", feed.FeedUri)));
                        }
                    }
                    feedResults = null;
                }
                catch(Exception exception)
                {
                    foreach (var shim in capturedGroup)
                    {
                        shim.TaskSource.TrySetException(exception);
                    }
                }
            }
        }




        #region Process a successful Weave Server feed result

        void ProcessSuccessfulFeedResult(FeedResult result, shim shim)
        {
            try
            {
                var feedUpdate = result.AsFeedUpdateResponse();
                shim.TaskSource.TrySetResult(feedUpdate);
            }
            catch (Exception e)
            {
                shim.TaskSource.TrySetException(e);
            }
        }

        #endregion




        #region Process an unmodified Weave Server feed result

        void ProcessUnmodifiedFeedResult(shim shim)
        {
            shim.TaskSource.TrySetResult(new FeedUpdateResponse { IsUnchanged = true });
        }

        #endregion




        #region Chunking/Batching feed code

        int batchSize = 20;
        IEnumerable<IList<shim>> ChunkWork(IList<shim> list)
        {
            List<IList<shim>> temp = new List<IList<shim>>();

            for (int i = 0; (i * batchSize) < list.Count; i++)
            {
                temp.Add(list.Skip(i * 20).Take(batchSize).ToList());
            }

            return temp;
        }

        #endregion
    }
}