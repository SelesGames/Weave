using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace SelesGames.WP.IsoStorage
{
    public class DataContractIsoStorageClient<T> : IsoStorageClient<T>
    {
        DataContractSerializer serializer = new DataContractSerializer(typeof(T));

        public DataContractIsoStorageClient(IEnumerable<Type> knownTypes)
        {
            serializer = new DataContractSerializer(typeof(T), knownTypes);
        }

        public DataContractIsoStorageClient()
        {
            serializer = new DataContractSerializer(typeof(T));
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
