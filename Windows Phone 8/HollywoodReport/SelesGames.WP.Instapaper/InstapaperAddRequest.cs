using System.Collections.Generic;

namespace SelesGames.Instapaper
{
    internal class InstapaperAddRequest
    {
        const string baseUri = "https://www.instapaper.com/api/add";

        List<WebRequestParameter> parameters = new List<WebRequestParameter>();

        InstapaperAddRequest(string username, string url)
        {
            parameters.Add(new WebRequestParameter("username", username));
            parameters.Add(new WebRequestParameter("url", url));
        }

        public InstapaperAddRequest WithPassword(string password)
        {
            parameters.Add(new WebRequestParameter("password", password));
            return this;
        }

        public InstapaperAddRequest WithTitle(string title)
        {
            parameters.Add(new WebRequestParameter("title", title));
            return this;
        }

        public InstapaperAddRequest WithSelection(string selection)
        {
            if (selection.Length > 200)
            {
                selection = selection.Substring(0, 200 - 3) + "...";
            }
            parameters.Add(new WebRequestParameter("selection", selection));
            return this;
        }

        public static InstapaperAddRequest CreateRequest(string username, string url)
        {
            return new InstapaperAddRequest(username, url);
        }

        public string GetUri()
        {
            return parameters.AddParametersToUri(baseUri);
        }
    }
}
