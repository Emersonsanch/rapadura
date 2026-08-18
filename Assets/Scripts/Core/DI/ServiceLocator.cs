using System;
using System.Collections.Generic;

namespace Rapadura.Core.DI
{
    /// <summary>
    /// Minimal service locator used as a lightweight dependency-injection mechanism.
    /// Managers register themselves here on bootstrap (see GameManager) and any system
    /// can resolve them by interface, avoiding scattered singletons and hard references
    /// between scripts. Scoped to the running application instance; cleared on shutdown
    /// so Domain Reload / Enter Play Mode Options don't leak stale references.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        /// <summary>Registers a service instance under the given contract type. Overwrites any previous registration.</summary>
        public static void Register<TService>(TService instance)
        {
            Services[typeof(TService)] = instance;
        }

        /// <summary>Resolves a previously registered service. Throws if it was never registered.</summary>
        public static TService Get<TService>()
        {
            if (Services.TryGetValue(typeof(TService), out object instance))
            {
                return (TService)instance;
            }

            throw new InvalidOperationException($"Service of type {typeof(TService).Name} is not registered.");
        }

        /// <summary>Attempts to resolve a service without throwing.</summary>
        public static bool TryGet<TService>(out TService instance)
        {
            if (Services.TryGetValue(typeof(TService), out object rawInstance))
            {
                instance = (TService)rawInstance;
                return true;
            }

            instance = default;
            return false;
        }

        public static bool IsRegistered<TService>()
        {
            return Services.ContainsKey(typeof(TService));
        }

        public static void Unregister<TService>()
        {
            Services.Remove(typeof(TService));
        }

        /// <summary>Clears every registration. Call on application quit or full scene teardown.</summary>
        public static void Clear()
        {
            Services.Clear();
        }
    }
}
