using System.Text;

namespace System.IO
{
    public static class StreamExtensionMethods
    {
        public static string GetReadStream(this Stream stream)
        {
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    reader.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("Exception in GetReadStream: \r\n{0}", ex);
                return null;
            }
        }

        public static string GetReadStream(this Stream stream, Encoding encoding)
        {
            try
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    string result = reader.ReadToEnd();
                    reader.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("Exception in GetReadStream: \r\n{0}", ex);
                return null;
            }
        }
    }
}
