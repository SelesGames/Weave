
namespace SelesGames.Rest.Protobuf
{
    public class ProtobufRestClient<T> : RestClient<T>
    {
        protected override T ReadObject(System.IO.Stream stream)
        {
            return ProtoBuf.Serializer.Deserialize<T>(stream);
        }
    }
}
