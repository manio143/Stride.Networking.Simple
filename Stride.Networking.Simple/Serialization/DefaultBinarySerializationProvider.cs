using System.IO;
using Stride.Core.Serialization;

namespace Stride.Networking.Simple.Serialization
{
    public class DefaultBinarySerializationProvider : INetworkSerializationProvider
    {
        public T Deserialize<T>(byte[] data)
        {
            using var stream = new MemoryStream(data);
            return BinarySerialization.Read<T>(stream);
        }

        public byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();
            BinarySerialization.Write(stream, obj);
            return stream.ToArray();
        }
    }
}
