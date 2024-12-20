using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Configurators;

// TODO: Implement PrioritizedServiceDescriptor to handle prioritizing already registered services.
internal class PrioritizedServiceDescriptor : ServiceDescriptor
{
    public int Priority { get; }

    public PrioritizedServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, int priority)
        : base(serviceType, implementationType, lifetime)
    {
        Priority = priority;
    }

    public PrioritizedServiceDescriptor(Type serviceType, object instance, int priority)
        : base(serviceType, instance)
    {
        Priority = priority;
    }

    public PrioritizedServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime, int priority)
        : base(serviceType, factory, lifetime)
    {
        Priority = priority;
    }
}