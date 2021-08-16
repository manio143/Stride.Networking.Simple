using Stride.Engine;
using TestGame.Network;

namespace TestGame
{
    // That's a quicky script - you can break in Update to inspect the state of network scripts in case they hang.
    public class DebuggingScript : SyncScript
    {
        public PingScript Ping;
        public ShuffleClient Shuffle;

        public override void Update()
        {
        }
    }
}
