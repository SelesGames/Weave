using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    internal static class StreamExtensionMethods
    {
        public async static Task<string> GetReadStream(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string result = await reader.ReadToEndAsync().ConfigureAwait(false);
                reader.Close();
                return result;
            }
        }

        public async static Task<string> GetReadStream(this Stream stream, Encoding encoding)
        {
            using (var reader = new StreamReader(stream, encoding))
            {
                string result = await reader.ReadToEndAsync().ConfigureAwait(false);
                reader.Close();
                return result;
            }
        }
    }
}