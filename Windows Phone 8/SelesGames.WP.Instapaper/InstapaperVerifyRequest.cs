using System.Collections.Generic;

namespace SelesGames.Instapaper
{
    internal class InstapaperVerifyRequest
    {
        const string baseUri = "https://www.instapaper.com/api/authenticate";

        public static string GetUri(string username, string password = null)
        {
            List<WebRequestParameter> parameters = new List<WebRequestParameter>();
            parameters.Add(new WebRequestParameter("username", username));
            if (!string.IsNullOrEmpty(password))
                parameters.Add(new WebRequestParameter("password", password));

            return parameters.AddParametersToUri(baseUri);
        }
    }
}
