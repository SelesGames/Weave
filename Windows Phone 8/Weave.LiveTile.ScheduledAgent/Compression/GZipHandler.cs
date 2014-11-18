using Common.Net.Http.Compression.Settings;
using System.IO;

namespace Common.WP.Compression
{
    internal class GZipHandler : CompressionHandler
    {
        public GZipHandler()
        {
            SupportedEncodings.Add("gzip");
        }

        public override Stream Compress(Stream inputStream)
        {
            return new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Compress, true);
            //var bytes = Noemax.Compression.CompressionFactory.GZip.Compress(inputStream, 1);
            //return new MemoryStream(bytes);
            //return new GZipOutputStream(inputStream);
        }

        public override Stream Decompress(Stream inputStream)
        {
            return new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Decompress, true);

            //var bytes = Noemax.Compression.CompressionFactory.GZip.Decompress(inputStream);
            //return new MemoryStream(bytes);
            //return new GZipInputStream(inputStream);
        }
    }
}
