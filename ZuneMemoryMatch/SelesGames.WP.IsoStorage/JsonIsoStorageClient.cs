using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace SelesGames.WP.IsoStorage
{
    public class JsonIsoStorageClient<T> : IsoStorageClient<T>
    {
        DataContractJsonSerializer serializer;

        public JsonIsoStorageClient(IEnumerable<Type> knownTypes)
        {
            serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
        }

        public JsonIsoStorageClient()
        {
            serializer = new DataContractJsonSerializer(typeof(T));
        }

        protected override T ReadObject(Stream stream)
        {
            return (T)serializer.ReadObject(stream);
        }

        protected override void WriteObject(T obj, Stream stream)
        {
            serializer.WriteObject(stream, obj);
        }
    }
}
