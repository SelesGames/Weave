
namespace Common.WP.Compression
{
    public class CompressionHandlerCollection : Common.Net.Http.Compression.Settings.CompressionHandlerCollection
    {
        public CompressionHandlerCollection()
        {
            this.Add(new GZipHandler());
            //this.Add(new DeflateHandler());
        }
    }
}
