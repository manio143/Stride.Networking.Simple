using Stride.Engine;

namespace Stride.Networking.Simple
{
    public static class GameExtensions
    {
        public static void AddNetworkClient(this Game game, out NetworkClientSystem networkClient)
        {
            networkClient = new NetworkClientSystem(game.Services);
            game.Services.AddService(networkClient);
            game.GameSystems.Add(networkClient);
        }
    }
}
