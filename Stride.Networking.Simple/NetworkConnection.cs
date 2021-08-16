using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stride.Engine.Network;

namespace Stride.Networking.Simple
{
    /// <summary>
    /// A network connection between server and client.
    /// </summary>
    public sealed class NetworkConnection : IDisposable
    {
        private SimpleSocket socket;
        private CancellationTokenSource cancellationTokenSource;

        internal Dictionary<Guid, NetworkScript> Handlers = new();

        internal NetworkConnection(SimpleSocket socket)
        {
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.cancellationTokenSource = new CancellationTokenSource();
            this.socket.Disconnected += _ => { this.cancellationTokenSource?.Cancel(); Closed?.Invoke(); };
        }

        internal CancellationToken CancellationToken => cancellationTokenSource.Token;

        public event Action Closed;

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            socket.Dispose();
        }

        internal async Task<LowLevelMessage> ReceiveAsync()
        {
            var guid = await this.socket.ReadStream.ReadGuidAsync();
            var dataLen = await this.socket.ReadStream.ReadInt32Async();
            var data = new byte[dataLen];

            while (dataLen > 0)
            {
                var read = await this.socket.ReadStream.ReadAsync(data, data.Length - dataLen, dataLen);
                dataLen -= read;
            }

            return new LowLevelMessage
            {
                HandlerId = guid,
                Data = data,
            };
        }

        internal async Task SendAsync(LowLevelMessage message)
        {
            await this.socket.WriteStream.WriteGuidAsync(message.HandlerId);
            await this.socket.WriteStream.WriteInt32Async(message.Data.Length);
            await this.socket.WriteStream.WriteAsync(message.Data, 0, message.Data.Length);
            await this.socket.WriteStream.FlushAsync();
        }
    }

    internal struct LowLevelMessage
    {
        public Guid HandlerId;
        public byte[] Data;
    }
}
