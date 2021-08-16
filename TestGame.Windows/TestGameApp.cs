using Stride.Engine;
using Stride.Networking.Simple;

namespace TestGame.Windows
{
    class TestGameApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                // Instantiate a connection to the server
                game.AddNetworkClient(out var networkClient);
                networkClient.Open("localhost", 25565);

                game.Run();
            }
        }
    }
}
