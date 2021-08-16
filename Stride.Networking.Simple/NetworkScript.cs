using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Engine;
using Stride.Networking.Simple.Channels;
using Stride.Networking.Simple.Serialization;

namespace Stride.Networking.Simple
{
    public class NetworkScript : StartupScript
    {
        private NetworkConnection networkConnection;
        private IServiceRegistry services;
        private INetworkSerializationProvider serialization;
        private Channel<byte[]> dataChannel = new();

        public NetworkScript()
        {
        }

        internal protected NetworkScript(IServiceRegistry serviceRegistry, NetworkConnection connection)
        {
            this.services = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
            this.networkConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.serialization = this.services.GetOrCreateNetworkSerializationProvider();
            this.networkConnection.Closed += () => IsCancelled = true;
        }

        [DataMemberIgnore]
        public new IServiceRegistry Services => this.services;

        [DataMemberIgnore]
        public Guid ScriptId { get; internal protected set; } = Guid.NewGuid();

        [DataMember(1000)]
        public OnError OnDeserializationError { get; set; }

        [DataMemberIgnore]
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// User script steps.
        /// </summary>
        internal protected virtual Task Execute() { return Task.CompletedTask; }

        protected async Task<bool> StartNewServerHandler<THandlerRequest>(THandlerRequest request)
            where THandlerRequest : Enum
        {
            var handlerRequest = HandlerRequest.Create(request);
            var data = handlerRequest.Serialize();
            await this.networkConnection.SendAsync(new LowLevelMessage
            {
                Data = data,
                HandlerId = this.ScriptId,
            });
            return await this.Receive<bool>();
        }

        public override void Start()
        {
            this.services = base.Services;
            var clientSystem = Services.GetService<NetworkClientSystem>();

            if (clientSystem == null || clientSystem.NetworkConnection == null)
            {
                throw new InvalidOperationException("A NetworkScript cannot be started without an initialized ClientSystem.");
            }

            this.serialization = this.services.GetOrCreateNetworkSerializationProvider();

            this.networkConnection = clientSystem.NetworkConnection;
            this.networkConnection.Handlers.Add(ScriptId, this);
            this.networkConnection.Closed += () => IsCancelled = true;

            Script.AddTask(Execute);
        }

        public override void Cancel()
        {
            base.Cancel();
            IsCancelled = true;
            this.networkConnection.Handlers.Remove(ScriptId);
        }

        public Task Send<TMessage>(TMessage message)
        {
            return this.networkConnection.SendAsync(new LowLevelMessage
            {
                HandlerId = this.ScriptId,
                Data = this.serialization.Serialize(message),
            });   
        }

        public async Task<TMessage> Receive<TMessage>()
        {
            do
            {
                var data = await this.dataChannel.Receive();
                try
                {
                    var message = this.serialization.Deserialize<TMessage>(data);
                    return message;
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    if (this.OnDeserializationError == OnError.Stop)
                        throw;
                }
            } while (true);
        }

        internal void Handle(byte[] data)
        {
            this.dataChannel.Send(data);
        }

        public enum OnError
        {
            Stop,
            Ignore,
        }
    }
}
