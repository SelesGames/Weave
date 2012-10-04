using System.IO;
using System.Threading.Tasks;

namespace System.Net
{
    internal static class WebRequestExtensionMethods
    {
        internal static async Task<string> GetReadStreamAsync(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string result = await reader.ReadToEndAsync().ConfigureAwait(false);
                reader.Close();
                return result;
            }
        }
    }
}
