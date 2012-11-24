using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GoogleReaderConnect
{
    /// <summary>
    /// This class helps connect to the Google Reader API to pull down a users list of feeds
    /// </summary>
    public class GoogleReader
    {
        string username = string.Empty;
        string password = string.Empty;     
        string authToken = string.Empty;




        #region Google Reader Parameters

        readonly string SERVICE = "reader";
        readonly string RETURN_FORMAT_XML = "xml";
        readonly string CLIENT_NAME = "WP7-GRC";
        readonly string AUTH_URI = "https://www.google.com/accounts/ClientLogin";
        readonly string SUBSCRIPTION_LIST_URI = "http://www.google.com/reader/api/0/subscription/list";

        #endregion // Google Reader Parameters




        #region Properties

        public AuthenticationResult AuthResult { get; private set; }
        public List<FeedInfo> Feeds { get; private set; }

        #endregion // Properties




        public GoogleReader(string username, string password)
        {
            this.username = username;
            this.password = password;
        }


        public enum AuthenticationResult
        {
            InvalidCredentials,
            OK
        }

        public async Task Authenticate()
        {
            string authUrl = String.Format("{0}?service={1}&Email={2}&Passwd={3}",
                AUTH_URI, SERVICE, HttpUtility.UrlEncode(username), password);

            var request = HttpWebRequest.CreateHttp(authUrl);
            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);
                await ParseAuthToken(response);
                AuthResult = AuthenticationResult.OK;
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ex.Response is HttpWebResponse)
                {
                    var statusCode = (HttpWebResponse)ex.Response;
                    if (statusCode.StatusCode == HttpStatusCode.Forbidden)
                        AuthResult = AuthenticationResult.InvalidCredentials;
                }
                else
                    throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        #region Helper Methods

        async Task ParseAuthToken(WebResponse response)
        {
            string content = string.Empty;

            using (var stream = response.GetResponseStream())
            {
                content = await stream.GetReadStreamAsync();
                stream.Close();
            }

            // Get the auth token
            string auth = string.Empty;
            try
            {
                auth = new Regex(@"Auth=(?<auth>\S+)").Match(content).Result("${auth}");
            }
            catch (Exception ex)
            {
                authToken = "Error: " + ex.Message;
            }

            // Validate token
            if (string.IsNullOrEmpty(auth))
            {
                authToken = "Error: Missing or invalid 'Auth' token.";
            }

            // Use this token in the header of each new request
            authToken = auth;
        }

        #endregion




        public async Task LoadSubscriptionList()
        {
            string returnFormat = RETURN_FORMAT_XML;
            string currentTime = GetUnixTimeNow().ToString();
            string client = CLIENT_NAME;

            string feedsUrl = String.Format("{0}?output={1}&ck={2}&client={3}",
                SUBSCRIPTION_LIST_URI, returnFormat, currentTime, client);

            var request = HttpWebRequest.CreateHttp(feedsUrl);
            request.Headers["Authorization"] = String.Format("GoogleLogin auth={0}", authToken);

            var response = await request.GetResponseAsync().ConfigureAwait(false);
            await ParseSubscriptionListReturn(response);
        }




        #region Helper Methods

        async Task ParseSubscriptionListReturn(WebResponse response)
        {
            string content = string.Empty;
            string feedUrl = string.Empty;

            using (var stream = response.GetResponseStream())
            {
                content = await stream.GetReadStreamAsync().ConfigureAwait(false);
                stream.Close();
            }

            List<FeedInfo> feeds = new List<FeedInfo>();

            XElement xmlItem = XElement.Parse(content);

            foreach (XElement item in xmlItem.Element("list").Elements("object"))
            {
                FeedInfo feed = new FeedInfo();

                // Get feed name and uri
                foreach (XElement stringChild in item.Elements("string"))
                {
                    // store the feed uri
                    if (stringChild.FirstAttribute.Value == "id")
                    {
                        string feedUri = stringChild.Value;
                        feedUrl = feedUri.Substring(5);//.Replace("feed/", "");
                        feed.Uri = feedUrl;
                    }

                    // store the feed name
                    if (stringChild.FirstAttribute.Value == "title")
                        feed.Name = stringChild.Value;
                }

                // Drill into the list of categories for this feed
                foreach (XElement category in item.Element("list").Elements("object"))
                {
                    foreach (XElement stringChild in category.Elements("string"))
                    {
                        if (stringChild.FirstAttribute.Value == "label")
                        {
                            feed.Categories = new List<string>();
                            feed.Categories.Add(stringChild.Value);
                        }
                    }
                }

                feeds.Add(feed);
            }

            this.Feeds = feeds;
        }

        long GetUnixTimeNow()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            long unixTime = (long)ts.TotalSeconds;
            return unixTime;
        }

        #endregion // Helper Methods
    }
}
