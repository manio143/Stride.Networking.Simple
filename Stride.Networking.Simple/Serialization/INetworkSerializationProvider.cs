namespace Stride.Networking.Simple.Serialization
{
    /// <summary>
    /// You can provide your custom serializer implementation and register it in the IServiceRegistry
    /// provided to the NetworkClientSystem/NetworkServerSystem.
    /// </summary>
    public interface INetworkSerializationProvider
    {
        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] data);
    }
}
