using System;
using Stride.Engine;

namespace Stride.Networking.Simple
{
    public static class GameExtensions
    {
        /// <summary>
        /// Adds <see cref="NetworkClientSystem"/> to the game.
        /// </summary>
        public static void AddNetworkClient(this Game game, out NetworkClientSystem networkClient)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            networkClient = new NetworkClientSystem(game.Services);
            game.Services.AddService(networkClient);
            game.GameSystems.Add(networkClient);
        }
    }
}
