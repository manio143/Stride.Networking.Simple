using System;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Core.Diagnostics;
using Stride.Networking.Simple;
using TestGame.Network;

namespace TestServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GlobalLogger.GlobalMessageLogged += (message) => Console.WriteLine(message.Text);

            var services = new ServiceRegistry();
            services.AddNetworkServer(out var networkServer)
                .AddNetworkHandler(PingRequest.Start, (srv, conn) => new PongScript(srv, conn))
                .AddNetworkHandler(ShuffleRequest.Start, (srv, conn) => new ShuffleServer(srv, conn));

            await networkServer.OpenAsync(25565);
            Console.WriteLine("Server started. Press any key to stop...");
            Console.ReadKey();
        }
    }
}
