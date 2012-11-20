using ProtoBuf;
using System.IO;

namespace SelesGames.IsoStorage.Protobuf
{
    public class ProtobufIsoStorageClient<T> : IsoStorageClient<T>
    {
        protected override T ReadObject(Stream stream)
        {
            return Serializer.Deserialize<T>(stream);
        }

        protected override void WriteObject(T obj, Stream stream)
        {
            Serializer.Serialize<T>(stream, obj);
        }
    }
}
