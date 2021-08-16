namespace Stride.Networking.Simple.Serialization
{
    public interface INetworkSerializationProvider
    {
        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] data);
    }
}
