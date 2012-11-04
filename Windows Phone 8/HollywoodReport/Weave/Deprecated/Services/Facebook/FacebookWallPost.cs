using System.Net;
using System.Text;
using System.Linq;

namespace weave.Services.Facebook
{
    public static class FacebookWallPostExtensions
    {
        public static FacebookWallPost ToFacebookWallPost(this NewsItem newsItem, string message)
        {
            return new FacebookWallPost
            {
                Name = newsItem.Title,
                Caption = newsItem.FeedSource.FeedName,
                //Description = newsItem.Description,
                Link = newsItem.Link,
                Message = message,
            };
        }
    }

    public class FacebookWallPost
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        //public string PictureLink { get; set; }

        public string GetPostParameters()
        {
            try
            {
                var sb = new StringBuilder();

                //var keys = new[] { "caption", "description", "link", "message", "name", "picture" };
                //var values = new[] { Caption, Description, Link, Message, Name, PictureLink };

                var keys = new[] { "message", "link", "caption", "description", "name" };
                var values = new[] { Message, Link, Caption, Description, Name };

                var kvps = values.Zip(keys, (Value, Key) => new { Value, Key })
                    .Where(o => !string.IsNullOrEmpty(o.Value))
                    .Select(o => string.Format("&{0}={1}", o.Key, HttpUtility.UrlEncode(o.Value)));

                foreach (var kvp in kvps)
                    sb.Append(kvp);

                if (sb.Length >= 1)
                    sb.Remove(0, 1);

                return (sb.ToString());
            }
            catch
            {
                return (string.Empty);
            }
        }
    }
}
