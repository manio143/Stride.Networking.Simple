using System;
using System.Collections.Generic;
using Stride.Core;

namespace Stride.Networking.Simple
{
    /// <summary>
    /// A wrapper around a dictionary that holds recipes on how to initialize message handlers.
    /// </summary>
    public class NetworkHandlerFactory
    {
        private Dictionary<object, Func<IServiceRegistry, NetworkConnection, NetworkScript>> factories = new();

        public bool TryGetHandlerInitializer(object request, out Func<IServiceRegistry, NetworkConnection, NetworkScript> initializer)
        {
            return this.factories.TryGetValue(request, out initializer);
        }

        public void RegisterHandler<THandlerRequest>(THandlerRequest request, Func<IServiceRegistry, NetworkConnection, NetworkScript> initializer)
            where THandlerRequest : Enum
        {
            this.factories.Add(request, initializer);
        }

        public void RemoveHandler<THandlerRequest>(THandlerRequest request)
            where THandlerRequest : Enum
        {
            this.factories.Remove(request);
        }
    }
}
