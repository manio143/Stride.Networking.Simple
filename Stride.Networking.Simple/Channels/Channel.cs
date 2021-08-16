using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stride.Networking.Simple.Channels
{
    /// <summary>
    /// This is an async one-directional channel for passing data.
    /// Stride has something similar, but I wanted an implementation that uses Task.Run (default thread pool)
    /// to run stuff, rather than microthreads.
    /// Basically you call Receive and if there's data available you get it right away, else you leave
    /// a continuation (C# compiler does that for you).
    /// Analogously if you call Send and no one is waiting then you leave data on a queue.
    /// If someone was waiting you pass the data to them and invoke the continuation on the thread pool.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
