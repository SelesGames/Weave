﻿using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Weave.FeedSearchService
{
    public class FeedSearchService
    {
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




        #region Private helper methods that either open a feed directly or search based on input

        // Call the RSS url directly, extract it's name and description
        async Task<FeedApiResult> DirectSearchForFeed(string feedUrl, CancellationToken cancelToken)
        {
            var client = new SmartHttpClient(compressionSettings: CompressionSettings.None);
            var response = await client.GetAsync(feedUrl, cancelToken);

            using (var responseStream = await response.ReadStream())
            using (var reader = XmlReader.Create(responseStream))
            {
                var feed = SyndicationFeed.Load(reader);
                var result = new FeedApiResult
                {
                    responseStatus = "200",
                    responseData = new ResponseData
                    {
                        entries = 
                            new List<Entry> 
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
                foreach (var entry in result.responseData.entries)
                    entry.url = feedUrl;
                return result;
            }
        }

        // Search using Google's RSS search service
        async Task<FeedApiResult> GoogleSearchForFeedsMatching(string searchString, CancellationToken cancelToken)
        {
            var url = string.Format(
                "http://ajax.googleapis.com/ajax/services/feed/find?q={0}&v=1.0",
                Uri.EscapeDataString(searchString));

            var client = new SmartHttpClient(ContentEncoderSettings.Json, compressionSettings: CompressionSettings.None);
            var jsonFormatter = client.Formatters.First();
            jsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/javascript"));

            var result = await client.GetAsync<FeedApiResult>(url, cancelToken);
            return result;
        }

        #endregion
    }
}
