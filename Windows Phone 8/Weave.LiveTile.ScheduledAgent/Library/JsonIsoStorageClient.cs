using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace SelesGames.IsoStorage
{
    internal class JsonIsoStorageClient<T> : IsoStorageClient<T>
    {
        public Encoding Encoding { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }

        public JsonIsoStorageClient()
        {
            Encoding = new UTF8Encoding(false, false);
            SerializerSettings = new JsonSerializerSettings();
        }

        protected override T ReadObject(Stream stream)
        {
            var serializer = JsonSerializer.Create(SerializerSettings);
            using (var streamReader = new StreamReader(stream, Encoding))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        protected override void WriteObject(T obj, Stream stream)
        {
            var serializer = JsonSerializer.Create(SerializerSettings);

            using (var ms = new MemoryStream())
            using (var streamWriter = new StreamWriter(ms, Encoding))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(jsonTextWriter, obj);
                jsonTextWriter.Flush();

                ms.Position = 0;
                ms.CopyTo(stream);

                jsonTextWriter.Close();
            }
        }
    }
}
