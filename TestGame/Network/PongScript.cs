using System.Threading.Tasks;
using Stride.Core;
using Stride.Networking.Simple;

namespace TestGame.Network
{
    // Server script - needs to have the constructor with those two parameters to correctly use it
    public class PongScript : NetworkScript
    {
        public PongScript(IServiceRegistry serviceRegistry, NetworkConnection connection)
            : base(serviceRegistry, connection)
        {
        }

        protected override async Task Execute()
        {
            while (!IsCancelled)
            {
                var response = await Receive<string>();
                await Send("Pong");
                Log.Info($"Received {response}; Sent Pong");
            }
        }
    }
}
