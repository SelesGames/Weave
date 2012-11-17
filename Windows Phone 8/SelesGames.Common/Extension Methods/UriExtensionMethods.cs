
namespace System.Net
{
    public static class UriExtensionMethods
    {
        #region Helper functions for creating Uri from string and WebRequest from Uri

        public static Uri ToUri(this string uri, UriKind uriKind = UriKind.Absolute)
        {
            if (!string.IsNullOrEmpty(uri) && Uri.IsWellFormedUriString(uri, uriKind))
                return new Uri(uri, uriKind);
            else
                return null;
        }

        public static WebRequest ToWebRequest(this Uri uri)
        {
            if (uri == null || uri.Scheme == null)
                return null;

            WebRequest request = null;
            try
            {
                //request = WebRequestCreator.ClientHttp.Create(uri);
                if (uri.Scheme == "http" || uri.Scheme == "https")
                    request = HttpWebRequest.Create(uri);
            }
            catch (Exception)
            {
            }
            return request;
        }

        #endregion
    }
}