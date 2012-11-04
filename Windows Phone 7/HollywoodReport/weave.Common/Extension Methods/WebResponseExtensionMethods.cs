using System.IO;
using System.Text;

namespace System.Net
{
    public static class WebResponseExtensionMethods
    {
        #region Helper functions converting WebResponse and Stream to a string value

        public static string GetResponseStreamAsString(this WebResponse response)
        {
            string result = null;
            using (var stream = response.GetResponseStream())
            {
                result = stream.GetReadStream();
                stream.Close();
            }
            return result;
        }

        public static string GetResponseStreamAsString(this WebResponse response, Encoding encoding)
        {
            string result = null;
            using (var stream = response.GetResponseStream())
            {
                result = stream.GetReadStream(encoding);
                stream.Close();
            }
            return result;
        }

        #endregion    
    }
}