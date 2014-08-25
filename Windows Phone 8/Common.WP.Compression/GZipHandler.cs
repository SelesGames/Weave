using Common.Compression;
using Ionic.Zlib;
using System.IO;

namespace Common.WP.Compression
{
    public class GZipHandler : CompressionHandler
    {
        public GZipHandler()
        {
            SupportedEncodings.Add("gzip");
        }

        public override Stream Compress(Stream inputStream)
        {
            return new GZipStream(inputStream, CompressionMode.Compress, true);
        }

        public override Stream Decompress(Stream inputStream)
        {
            return new GZipStream(inputStream, CompressionMode.Decompress, true);
        }
    }
}
