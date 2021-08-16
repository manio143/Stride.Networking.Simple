using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stride.Networking.Simple.Channels
{
    internal class Channel<T>
    {
        internal readonly object _lock = new();
        internal readonly Queue<T> dataQueue = new();
        internal readonly Queue<Awaiter<T>> awaiterQueue = new();

        public AwaiterBuilder<T> Receive()
        {
            return new AwaiterBuilder<T>(this);
        }

        public void Send(T data)
        {
            lock (_lock)
            {
                if (awaiterQueue.Count > 0)
                {
                    var awaiter = awaiterQueue.Dequeue();
                    awaiter.Result = data;
                    awaiter.IsCompleted = true;

                    if (awaiter.Continuation == null)
                        awaiter.WaitHandle.Wait();

                    Task.Run(awaiter.Continuation);
                }
                else
                {
                    dataQueue.Enqueue(data);
                }
            }
        }

        internal class AwaiterBuilder<T>
        {
            private Channel<T> channel;
            public AwaiterBuilder(Channel<T> channel) => this.channel = channel;

            public Awaiter<T> GetAwaiter()
            {
                lock (channel._lock)
                {
                    if (channel.dataQueue.Count > 0)
                        return new Awaiter<T>(channel.dataQueue.Dequeue());

                    var awaiter = new Awaiter<T>();
                    channel.awaiterQueue.Enqueue(awaiter);
                    return awaiter;
                }
            }
        }
    }
}
