
namespace weave.Services.Twitter
{
    public static class TwitterSettings
    {
        // Make sure you set your own ConsumerKey and Secret from dev.twitter.com
        public static string ConsumerKey { get; set; }
        public static string ConsumerKeySecret { get; set; }
        
        public static string RequestTokenUri = "https://api.twitter.com/oauth/request_token";
        public static string OAuthVersion = "1.0";
        public static string CallbackUri = "http://www.bing.com";
        public static string AuthorizeUri = "https://api.twitter.com/oauth/authorize";
        public static string AccessTokenUri = "https://api.twitter.com/oauth/access_token";
    }
}
