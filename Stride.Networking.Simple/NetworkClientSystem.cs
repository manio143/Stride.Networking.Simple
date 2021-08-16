using System;
using System.Threading;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Diagnostics;
using Stride.Engine.Network;
using Stride.Games;

namespace Stride.Networking.Simple
{
    /// <summary>
    /// Game system for the network client.
    /// </summary>
    public class NetworkClientSystem : GameSystemBase
    {
        private static ILogger logger = GlobalLogger.GetLogger(nameof(NetworkClientSystem));

        private SemaphoreSlim waitHandle = new SemaphoreSlim(0, 1);
        private TimeSpan waitTimeout = TimeSpan.FromSeconds(1);
        internal NetworkConnection NetworkConnection { get; private set; }
        public NetworkClientSystem([NotNull] IServiceRegistry registry) : base(registry)
        {
        }

        /// <summary>
        /// Opens a connection to the server.
        /// </summary>
        /// <param name="target">IP address or host name</param>
        /// <param name="port">Port</param>
        /// <param name="waitForConnectionTimeout">Custom timeout, 1 second by default.</param>
        public void Open(string target, int port, TimeSpan? waitForConnectionTimeout = null)
        {
            if (waitForConnectionTimeout != null)
                this.waitTimeout = waitForConnectionTimeout.Value;

            var socket = new SimpleSocket();
            socket.Connected += this.StartProcessingConnectionAsync;
            socket.StartClient(target, port);
        }

        protected override void Destroy()
        {
            base.Destroy();
            NetworkConnection.Dispose();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (NetworkConnection == null)
                if(!waitHandle.Wait(this.waitTimeout))
                    logger.Error("Opening connection to the server was unsuccessful");
        }

        private async void StartProcessingConnectionAsync(SimpleSocket socket)
        {
            this.NetworkConnection = new NetworkConnection(socket);
            waitHandle.Release();

            while (!NetworkConnection.CancellationToken.IsCancellationRequested)
            {
                NetworkScript handler;
                var message = await this.NetworkConnection.ReceiveAsync();

                if (NetworkConnection.Handlers.TryGetValue(message.HandlerId, out handler))
                {
                    handler.Handle(message.Data);
                }
            }
        }
    }
}
