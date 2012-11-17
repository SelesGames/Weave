using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>
    /// Helper functions converting WebResponse and Stream to a string value
    /// </summary>
    internal static class WebResponseExtensionMethods
    {
        public async static Task<string> GetResponseStreamAsString(this WebResponse response)
        {
            string result = null;
            using (var stream = response.GetResponseStream())
            {
                result = await stream.GetReadStream().ConfigureAwait(false);
                stream.Close();
            }
            return result;
        }

        public async static Task<string> GetResponseStreamAsString(this WebResponse response, Encoding encoding)
        {
            string result = null;
            using (var stream = response.GetResponseStream())
            {
                result = await stream.GetReadStream(encoding).ConfigureAwait(false);
                stream.Close();
            }
            return result;
        }
    }
}