using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace SelesGames.Rest.JsonDotNet
{
    public class JsonDotNetRestClient : RestClient
    {
        public Encoding Encoding { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }

        public JsonDotNetRestClient()
        {
            Headers.Accept = "application/json";
            Headers.ContentType = "application/json";

            Encoding = new UTF8Encoding(false, false);
            SerializerSettings = new JsonSerializerSettings();
        }

        protected override T ReadObject<T>(System.IO.Stream stream)
        {
            var serializer = JsonSerializer.Create(SerializerSettings);
            using (var streamReader = new StreamReader(stream, Encoding))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        protected override void WriteObject<T>(Stream writeStream, T obj)
        {
            throw new NotImplementedException();
            //var serializer = JsonSerializer.Create(SerializerSettings);
            //using (var streamReader = new StreamReader(stream, Encoding))
            //using (var jsonTextReader = new JsonTextReader(streamReader))
            //{
            //    return serializer.Deserialize<T>(jsonTextReader);
            //}
        }
    }
}
