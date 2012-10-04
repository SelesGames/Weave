using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Xml.Linq;
using Hammock;

namespace weave.Services.Twitter
{
    public static class TwitterTimelineService
    {
        const String MAX_COUNT = "100";
        static long sinceId = 0;

        public static IObservable<IEnumerable<Tweet>> GetTimeLine(this TwitterClient client)
        {
            return Observable.Create<IEnumerable<Tweet>>(observer =>
            {
                IDisposable disp = Disposable.Empty;

                try
                {
                    var request = new RestRequest
                    {
                        Credentials = client.Credentials,
                        Path = "/statuses/friends_timeline.xml",
                    };

                    request.AddParameter("count", MAX_COUNT);

                    if (sinceId != 0)
                        request.AddParameter("since_id", sinceId.ToString());

                    request.AddParameter("include_rts", "1");


                    client.Client.BeginRequest(request, new RestCallback(
                        (restRequest, response, userState) =>
                        {
                            try
                            {
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    var result = ParseTweets(response);
                                    observer.OnNext(result);
                                    observer.OnCompleted();
                                }
                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    observer.OnError(new UnauthorizedCredentialsException());
                                }
                                else
                                {
                                    observer.OnError(new Exception(response.StatusCode.ToString()));
                                }
                            }
                            catch (Exception exception)
                            {
                                observer.OnError(exception);
                            }
                        }));
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return disp;
            });
        }

        static IEnumerable<Tweet> ParseTweets(RestResponse response)
        {
            var xmlElement = XElement.Parse(response.Content);
            var list = (from item in xmlElement.Elements("status")
                        select new Tweet
                        {
                            UserName = GetChildElementValue(item, "user", "screen_name"),
                            DisplayUserName = GetChildElementValue(item, "user", "name"),
                            Text = (string)item.Element("text"),
                            CreatedAt = GetCreatedDate((string)item.Element("created_at")),
                            ImageUrl = GetChildElementValue(item, "user", "profile_image_url"),
                            Id = (long)item.Element("id"),
                            NewTweet = true,
                            Source = (string)item.Element("source"),
                        })
                        .ToList();
            return list;
        }



        #region Helper Methods

        static string GetChildElementValue(XElement itemElement, string parentElement, string childElement)
        {
            var userElement = itemElement.Element(parentElement);
            if (userElement == null)
                return String.Empty;

            var iteem = userElement.Element(childElement);
            if (iteem == null)
                return String.Empty;

            return iteem.Value;
        }

        static string GetCreatedDate(string createdDate)
        {
            DateTime date = ParseDateTime(createdDate);
            return date.ToShortDateString() + " " + date.ToShortTimeString();
        }

        static DateTime ParseDateTime(string date)
        {
            var dayOfWeek = date.Substring(0, 3).Trim();
            var month = date.Substring(4, 3).Trim();
            var dayInMonth = date.Substring(8, 2).Trim();
            var time = date.Substring(11, 9).Trim();
            var offset = date.Substring(20, 5).Trim();
            var year = date.Substring(25, 5).Trim();
            var dateTime = string.Format("{0}-{1}-{2} {3}", dayInMonth, month, year, time);
            var ret = DateTime.Parse(dateTime).ToLocalTime();

            return ret;
        }

        #endregion
    }
}
