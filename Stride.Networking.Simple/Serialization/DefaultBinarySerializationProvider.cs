using System.IO;
using Stride.Core.Serialization;

namespace Stride.Networking.Simple.Serialization
{
    /// <summary>
    /// Stride serialization - custom classes need <see cref="Stride.Core.DataContractAttribute"/> to be used with it.
    /// Also note that's it requires the client and server to use exactly the same version of a type.
    /// </summary>
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
