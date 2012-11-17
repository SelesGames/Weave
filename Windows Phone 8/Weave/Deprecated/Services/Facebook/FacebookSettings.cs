
namespace weave.Services.Facebook
{
    public static class FacebookSettings
    {
        public static string AppId { get; set; }
        public static string ApiKey { get; set; }
        public static string AppSecret { get; set; }
        
        public static string RequestTokenUri = "https://api.twitter.com/oauth/request_token";
        public static string OAuthVersion = "2.0";
        public static string CallbackUri = "http://www.selesgames.com/";
        public static string AuthorizeUri = "https://api.twitter.com/oauth/authorize";
        public static string AccessTokenUri = "https://api.twitter.com/oauth/access_token";
    }
}
