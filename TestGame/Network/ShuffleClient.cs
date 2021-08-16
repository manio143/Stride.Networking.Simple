using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Networking.Simple;

namespace TestGame.Network
{
    // Client script
    public class ShuffleClient : NetworkScript
    {
        public float Range { get; set; }

        protected override async Task Execute()
        {
            // This will tell the server to instantiate a new ShuffleServer
            await StartNewServerHandler(ShuffleRequest.Start);

            Vector3 shuffleOffset;
            while (!IsCancelled)
            {
                await Send(Range);
                shuffleOffset = await Receive<Vector3>();

                // In order to safely modify entities we need to get onto the main thread
                await Script.NextFrame();
                this.Entity.Transform.Position += shuffleOffset;
            }
        }
    }
}
