using System;
using Stride.Core;
using Stride.Networking.Simple.Serialization;

namespace Stride.Networking.Simple
{
    public static class ServiceRegistryExtensions
    {
        internal static INetworkSerializationProvider GetOrCreateNetworkSerializationProvider(this IServiceRegistry services)
        {
            INetworkSerializationProvider provider = services.GetService<INetworkSerializationProvider>();
            if (provider == null)
            {
                provider = new DefaultBinarySerializationProvider();
                services.AddService(provider);
            }
            return provider;
        }

        public static IServiceRegistry AddNetworkHandler<THandlerRequest>(this IServiceRegistry services, THandlerRequest request, Func<IServiceRegistry, NetworkConnection, NetworkScript> initializer)
            where THandlerRequest : Enum
        {
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
            networkServer = new NetworkServerSystem(services);
            services.AddService(networkServer);
            return services;
        }
    }
}
