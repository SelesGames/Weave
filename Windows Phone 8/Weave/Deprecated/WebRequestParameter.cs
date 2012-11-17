using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

namespace weave.Services.Instapaper
{
    public class WebRequestParameter
    {
        public WebRequestParameter(string parameterName, string value)
        {
            ParameterName = parameterName;
            Value = value;
        }

        public string ParameterName { get; set; }
        public string Value { get; set; }
    }

    public static class WebRequestParameterExtensions
    {
        public static string AddParametersToUri(this IEnumerable<WebRequestParameter> parameters, string baseUri)
        {
            if (parameters == null || baseUri == null)
                return null;

            StringBuilder sb = new StringBuilder();
            foreach (var parameter in parameters)
            {
                sb.Append('&');
                sb.Append(parameter.ParameterName);
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(parameter.Value));
            }
            if (parameters.Count() > 0)
            {
                sb.Remove(0, 1);
                sb.Insert(0, "?");
            }
            sb.Insert(0, baseUri);
            return sb.ToString();
        }
    }
}
