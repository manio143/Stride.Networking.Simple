using System;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Networking.Simple;

namespace TestGame.Network
{
    /// <summary>
    /// A script which when receives a float x value returns a random Vector3 in the range (-x, x).
    /// </summary>
    // Server script - needs to have the constructor with those two parameters to correctly use it
    public class ShuffleServer : NetworkScript
    {
        private Random Random = new();
        public ShuffleServer(IServiceRegistry serviceRegistry, NetworkConnection connection)
            : base(serviceRegistry, connection)
        {
        }

        protected override async Task Execute()
        {
            while (!IsCancelled)
            {
                var range = await Receive<float>();
                Log.Debug($"Received Shuffle request for ({range})");
                // do calculations
                var offset = new Vector3
                {
                    X = (float)Random.NextDouble() * 2 * range - range,
                    Y = (float)Random.NextDouble() * 2 * range - range,
                    Z = (float)Random.NextDouble() * 2 * range - range,
                };
                Log.Info($"Sending Shuffle ({offset})");
                await Send(offset);
            }
        }
    }
}
