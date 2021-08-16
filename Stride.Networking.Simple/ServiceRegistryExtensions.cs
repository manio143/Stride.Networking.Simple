using System;
using Stride.Core;
using Stride.Networking.Simple.Serialization;

namespace Stride.Networking.Simple
{
    public static class ServiceRegistryExtensions
    {
        internal static INetworkSerializationProvider GetOrCreateNetworkSerializationProvider(this IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            INetworkSerializationProvider provider = services.GetService<INetworkSerializationProvider>();
            if (provider == null)
            {
                provider = new DefaultBinarySerializationProvider();
                services.AddService(provider);
            }
            return provider;
        }

        /// <summary>
        /// Adds a network handler. When new request is received its value will be compared with <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="THandlerRequest">Type of the request (with enum constraint).</typeparam>
        /// <param name="services"></param>
        /// <param name="request">Request object.</param>
        /// <param name="initializer">Factory function for the handler.</param>
        /// <returns>The registry for chaining calls.</returns>
        public static IServiceRegistry AddNetworkHandler<THandlerRequest>(this IServiceRegistry services, THandlerRequest request, Func<IServiceRegistry, NetworkConnection, NetworkScript> initializer)
            where THandlerRequest : Enum
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            NetworkHandlerFactory factory = services.GetService<NetworkHandlerFactory>();
            if (factory == null)
            {
                factory = new NetworkHandlerFactory();
                services.AddService(factory);
            }

            factory.RegisterHandler(request, initializer);
            return services;
        }

        public static IServiceRegistry AddNetworkServer(this IServiceRegistry services, out NetworkServerSystem networkServer)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            networkServer = new NetworkServerSystem(services);
            services.AddService(networkServer);
            return services;
        }
    }
}
