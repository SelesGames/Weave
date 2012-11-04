using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SelesGames.Rest;

namespace Weave.FeedSearchService
{
    public class FeedSearchService
    {
        RestClient<FeedApiResult> googleSearchClient, directSearchClient;

        public FeedSearchService()
        {
            googleSearchClient = new JsonRestClient<FeedApiResult>();
            directSearchClient = new DelegateRestClient<FeedApiResult>(stream =>
            {
                using (var reader = XmlReader.Create(stream))
                {
                    var feed = SyndicationFeed.Load(reader);
                    var result = new FeedApiResult
                    {
                        responseStatus = "200",
                        responseData = new ResponseData
                        {
                            entries = new List<Entry> 
                            { 
                                new Entry 
                                { 
                                    //url = feedUrl, 
                                    title = feed.Title.Text, 
                                    contentSnippet = feed.Description.Text,
                                }
                            }
                        }
                    };
                    reader.Close();
                    return result;
                }
            });
        }

        public async Task<FeedApiResult> SearchForFeedsMatching(string searchString, CancellationToken cancelToken)
        {
            FeedApiResult result;
            try
            {
                if (Uri.IsWellFormedUriString(searchString, UriKind.Absolute))
                    result = await DirectSearchForFeed(searchString, cancelToken);

                else
                    result = await GoogleSearchForFeedsMatching(searchString, cancelToken);
            }
            catch
            {
                result = new FeedApiResult { responseStatus = "999" };
            }
            return result;
        }

        // Call the RSS url directly, extract it's name and description
        async Task<FeedApiResult> DirectSearchForFeed(string feedUrl, CancellationToken cancelToken)
        {
            var result = await directSearchClient.GetAsync(feedUrl, cancelToken);
            foreach (var entry in result.responseData.entries)
                entry.url = feedUrl;
            return result;
        }

        // Search using Google's RSS search service
        async Task<FeedApiResult> GoogleSearchForFeedsMatching(string searchString, CancellationToken cancelToken)
        {
            var url = string.Format(
                "http://ajax.googleapis.com/ajax/services/feed/find?q={0}&v=1.0",
                HttpUtility.UrlEncode(searchString));

            var result = await googleSearchClient.GetAsync(url, cancelToken);
            return result;
        }
    }
}
