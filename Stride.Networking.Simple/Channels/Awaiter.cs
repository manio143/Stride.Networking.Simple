using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stride.Networking.Simple.Channels
{
    internal class Awaiter<T> : INotifyCompletion
    {
        public Awaiter() { }

        public Awaiter(T data)
        {
            IsCompleted = true;
            Result = data;
        }

        internal Action Continuation { get; set; }

        internal SemaphoreSlim WaitHandle { get; } = new SemaphoreSlim(0, 1);

        internal T Result { get; set; }

        public bool IsCompleted { get; internal set; }

        public T GetResult() => Result;

        public void OnCompleted(Action continuation)
        {
            this.Continuation = continuation;
            WaitHandle.Release();
        }
    }
}
