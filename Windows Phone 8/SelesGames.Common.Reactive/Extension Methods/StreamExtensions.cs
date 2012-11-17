
namespace System.IO
{
    public static class StreamExtensions
    {
        public static MemoryStream ToMemoryStream(this Stream stream)
        {
            var br = new BinaryReader(stream);
            var outputStream = new MemoryStream();
            var buffer = new byte[1024];
            int cb;
            while ((cb = br.Read(buffer, 0, buffer.Length)) > 0)
            {
                outputStream.Write(buffer, 0, cb);
            }
            outputStream.Position = 0;
            return outputStream;
        }
    }
}
