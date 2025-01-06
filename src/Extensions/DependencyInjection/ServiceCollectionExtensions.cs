using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Reflection;

namespace FluentInjections.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a service to the container only if the condition is true.
    /// </summary>
    public static IServiceCollection AddConditional<TService>(this IServiceCollection services, Func<bool> condition, ServiceLifetime lifetime)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(condition, nameof(condition));

        if (condition())
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), lifetime));
        }

        return services;
    }

    /// <summary>
    /// Replaces an existing service with a new implementation.
    /// </summary>
    public static IServiceCollection ReplaceService<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory, ServiceLifetime lifetime)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(implementationFactory, nameof(implementationFactory));

        var serviceType = typeof(TService);
        services.RemoveAll(serviceType);
        services.Add(new ServiceDescriptor(typeof(TService), implementationFactory, lifetime));
        return services;
    }

    /// <summary>
    /// Registers all services in an assembly based on a filter predicate.
    /// </summary>
    public static IServiceCollection RegisterAssemblyServices(this IServiceCollection services, Assembly assembly, Func<Type, bool> filter)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(assembly, nameof(assembly));
        Guard.NotNull(filter, nameof(filter));

        var types = assembly.GetTypes()
                            .Where(filter);

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                services.Add(new ServiceDescriptor(@interface, type, Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient));
            }

            if (type.IsClass && !type.IsAbstract)
            {
                services.Add(new ServiceDescriptor(type, type, Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient));
            }
        }

        return services;
    }

    /// <summary>
    /// Registers multiple decorators for a given service type.
    /// </summary>
    public static IServiceCollection RegisterDecorators<TService>(this IServiceCollection services, params Type[] decorators)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(decorators, nameof(decorators));

        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService));

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Service of type '{typeof(TService).Name}' is not registered.");
        }

        Func<IServiceProvider, object> originalImplementationFactory = descriptor.ImplementationFactory
            ?? throw new InvalidOperationException($"Service of type '{typeof(TService).Name}' is not registered.");

        foreach (var decorator in decorators)
        {
            services.Remove(descriptor);
            descriptor = new ServiceDescriptor(typeof(TService), provider =>
            {
                var inner = (TService)originalImplementationFactory(provider);
                var decoratorInstance = (IDecorator<TService>)provider.GetRequiredService(decorator);
                return decoratorInstance.Decorate(inner)
                    ?? throw new InvalidOperationException($"Decorator of service type '{typeof(TService).Name}' could not be decorated.");
            }, descriptor.Lifetime);

            services.Add(descriptor);
        }

        return services;
    }

    /// <summary>
    /// Registers a service as scoped only if it has not already been registered.
    /// </summary>
    public static IServiceCollection AddScopedIfNotRegistered<TService>(this IServiceCollection services)
    {
        Guard.NotNull(services, nameof(services));

        if (!services.Any(d => d.ServiceType == typeof(TService)))
        {
            services.AddScoped(typeof(TService));
        }

        return services;
    }
}
