
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

        #endregion
    }
}