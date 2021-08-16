using System;
using System.Threading.Tasks;
using Stride.Networking.Simple;

namespace TestGame.Network
{
    // Client script
    public class PingScript : NetworkScript
    {
        protected override async Task Execute()
        {
            // This will tell the server to instantiate a new PongScript
            await StartNewServerHandler(PingRequest.Start);

            while (!IsCancelled)
            {
                await Send("Ping");
                var response = await Receive<string>();
                Log.Info("Sent Ping; Received " + response);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
