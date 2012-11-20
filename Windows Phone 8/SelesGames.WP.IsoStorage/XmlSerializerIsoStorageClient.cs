using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;


namespace SelesGames.IsoStorage
{
    public class XmlSerializerIsoStorageClient<T> : IsoStorageClient<T>
    {
        XmlSerializer serializer;

        public XmlSerializerIsoStorageClient(IEnumerable<Type> knownTypes)
        {
            serializer = new XmlSerializer(typeof(T), knownTypes.ToArray());
        }

        public XmlSerializerIsoStorageClient()
        {
            serializer = new XmlSerializer(typeof(T));
        }

        protected override T ReadObject(Stream stream)
        {
            return (T)serializer.Deserialize(stream);
        }

        protected override void WriteObject(T obj, Stream stream)
        {
            serializer.Serialize(stream, obj);
        }
    }
}
