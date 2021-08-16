using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Stride.Core;

namespace Stride.Networking.Simple
{
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

        private class EnumEqualityComparer : EqualityComparer<object>
        {
            public override bool Equals(object x, object y)
            {
                return (x as IComparable).CompareTo(y) == 0;
            }

            public override int GetHashCode([DisallowNull] object obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
