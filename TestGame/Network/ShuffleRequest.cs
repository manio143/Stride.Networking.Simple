using Stride.Core;

namespace TestGame.Network
{
    /// <summary>
    /// Enum used to link <see cref="ShuffleClient"/> and <see cref="ShuffleServer"/>.
    /// </summary>
    [DataContract]
    public enum ShuffleRequest
    {
        Start
    }
}
