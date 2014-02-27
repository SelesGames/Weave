using Common.Compression;
using ICSharpCode.SharpZipLib.GZip;
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
            return new GZipOutputStream(inputStream);
        }

        public override Stream Decompress(Stream inputStream)
        {
            return new GZipInputStream(inputStream);
        }
    }
}
