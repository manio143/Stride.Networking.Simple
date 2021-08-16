using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Core.Diagnostics;
using Stride.Engine.Network;

namespace Stride.Networking.Simple
{
    public sealed class NetworkServerSystem : IDisposable
    {
        private static ILogger logger = GlobalLogger.GetLogger(nameof(NetworkServerSystem));

        private IServiceRegistry services;
        private bool initialized;
        private SimpleSocket serverSocket = new();
        private NetworkHandlerFactory handlerFactory;

        private object stateLock = new();
        private List<NetworkConnection> connections = new();
        private List<Task> connectionWorkers = new();

        public NetworkServerSystem(IServiceRegistry services)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.services.GetServiceLate<NetworkHandlerFactory>(factory => this.handlerFactory = factory);
        }

        public event Action<NetworkConnection> ConnectionOpened;
        public event Action<NetworkConnection> ConnectionClosed;

        public async Task<bool> OpenAsync(int port)
        {
            if (this.initialized)
                return false;

            this.serverSocket.Connected += this.OnNewConnection;
            await this.serverSocket.StartServer(port, singleConnection: false, retryCount: 2);
            this.initialized = true;
            return true;
        }

        public void Dispose() => this.serverSocket.Dispose();

        private void OnNewConnection(SimpleSocket socket)
        {
            logger.Info($"New connection opened: {socket.RemoteAddress} ({socket.RemotePort})");
            var connection = new NetworkConnection(socket);
            this.ConnectionOpened?.Invoke(connection);
            var worker = this.StartProcessingAsync(connection);

            connection.Closed += () =>
            {
                lock (this.stateLock)
                {
                    connections.Remove(connection);
                    connectionWorkers.Remove(worker);
                }
                this.ConnectionClosed?.Invoke(connection);
            };

            lock (this.stateLock)
            {
                connections.Add(connection);
                connectionWorkers.Add(worker);
            }
        }

        private async Task StartProcessingAsync(NetworkConnection connection)
        {
            while (!connection.CancellationToken.IsCancellationRequested)
            {
                NetworkScript handler;
                var message = await connection.ReceiveAsync();
                logger.Debug($"Received message for {{{message.HandlerId}}}");

                if (connection.Handlers.TryGetValue(message.HandlerId, out handler))
                {
                    handler.Handle(message.Data);
                }
                else
                {
                    bool handlerStarted = false;
                    handler = new NetworkScript(this.services, connection);
                    try
                    {
                        var handlerRequest = HandlerRequest.Deserialize(message.Data);

                        if (this.handlerFactory.TryGetHandlerInitializer(handlerRequest.Instantiate(), out var initializer))
                        {
                            handler = initializer(this.services, connection);
                            connection.Handlers.Add(message.HandlerId, handler);
                            _ = Task.Run(handler.Execute); // start execution on the thread pool
                            handlerStarted = true;
                        }
                    }
                    catch (Exception ex) when (ex is not OutOfMemoryException)
                    {
                        logger.Error("An error occurred when processing request.", ex);
                    }
                    finally
                    {
                        handler.ScriptId = message.HandlerId;
                        await handler.Send(handlerStarted);
                    }
                }
            }
        }
    }
}
