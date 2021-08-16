using Stride.Core;

namespace TestGame.Network
{
    /// <summary>
    /// Enum used to link <see cref="PingScript"/> and <see cref="PongScript"/>.
    /// </summary>
    [DataContract]
    public enum PingRequest
    {
        Start
    }
}
