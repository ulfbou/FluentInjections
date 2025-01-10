// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections;
using FluentInjections.Extensions;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Reflection;

namespace FluentInjections;

public static class NetCoreNamedServiceExtensions
{
    /// <summary>
    /// Adds FluentInjections to the service collection by scanning the specified assemblies for <see cref="IModule{IServiceConfigurator}"/> implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for FluentInjections.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown if FluentInjections has already been initialized.</exception>
    public static IServiceCollection AddFluentInjections(this IServiceCollection services, params Assembly[]? assemblies)
    {
        DependencyInjection.AddFluentInjections(services, assemblies);
        return services;
    }

    /// <summary>
    /// Adds FluentInjections to the service collection by scanning the specified assemblies for <see cref="IModule{IServiceConfigurator}"/> implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for FluentInjections.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown if FluentInjections has already been initialized.</exception>


    /// <summary>
    /// Adds FluentInjections to the service collection by scanning the specified assemblies for <see cref="IModule{IMiddlewareConfigurator}"/> implementations.
    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[]? assemblies)
    {
        DependencyInjection.UseFluentInjections(app, assemblies);
        return app;
    }

    #region Register
    internal static readonly Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>> NamedServices = new();
    internal static readonly Dictionary<Type, ServiceBindingDescriptor> UnnamedServices = new();

    // Register service binding descriptor
    internal static void Register(this IServiceCollection services, ServiceBindingDescriptor descriptor)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(descriptor, nameof(descriptor));

        lock (NamedServices)
        {
            if (string.IsNullOrEmpty(descriptor.Name))
            {
                UnnamedServices[descriptor.BindingType] = descriptor;
            }
            else
            {
                if (!NamedServices.ContainsKey(descriptor.Name))
                {
                    NamedServices[descriptor.Name] = new Dictionary<Type, ServiceBindingDescriptor>();
                }

                NamedServices[descriptor.Name][descriptor.BindingType] = descriptor;
            }
        }

        AddServiceDescriptor(services, descriptor);
    }

    private static void AddServiceDescriptor(IServiceCollection services, ServiceBindingDescriptor descriptor)
    {
        if (descriptor.Instance is not null)
        {
            services.Add(new ServiceDescriptor(descriptor.BindingType, provider =>
            {
                descriptor.Configure?.Invoke(descriptor.Instance);
                return descriptor.Instance;
            }, ServiceLifetime.Singleton));
        }
        else if (descriptor.Factory is not null)
        {
            services.Add(new ServiceDescriptor(descriptor.BindingType, provider =>
            {
                var service = descriptor.Factory(provider);
                descriptor.Configure?.Invoke(service);
                return service;
            }, descriptor.Lifetime));
        }
        else if (descriptor.ImplementationType is not null)
        {
            if (descriptor.Parameters.Any())
            {
                services.Add(new ServiceDescriptor(descriptor.BindingType, provider =>
                {
                    var service = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, descriptor.Parameters.Values.ToArray());
                    descriptor.Configure?.Invoke(service);
                    return service;
                }, descriptor.Lifetime));
            }
            else
            {
                if (descriptor.BindingType != descriptor.ImplementationType && !services.Any(sd => sd.ServiceType == descriptor.BindingType))
                {
                    services.Add(new ServiceDescriptor(descriptor.BindingType, descriptor.ImplementationType, descriptor.Lifetime));
                }

                services.Add(new ServiceDescriptor(descriptor.ImplementationType, descriptor.ImplementationType, descriptor.Lifetime));
            }
        }
        else
        {
            throw new InvalidOperationException("ServiceBindingDescriptor must have an Instance, Factory, or ImplementationType defined.");
        }
    }
    #endregion

    public static TService? GetNamedService<TService>(this IServiceProvider provider, string name) where TService : notnull
    {
        var service = GetNamedService(provider, typeof(TService), name);

        if (service is not null)
        {
            return (TService)service;
        }

        return default;
    }

    public static object? GetNamedService(this IServiceProvider provider, Type serviceType, string name)
    {
        Guard.NotNull(provider, nameof(provider));
        Guard.NotNullOrEmpty(name, nameof(name));

        lock (NamedServices)
        {
            if (NamedServices.TryGetValue(name, out var services) && services.TryGetValue(serviceType, out var descriptor))
            {
                if (descriptor.Instance is not null)
                {
                    descriptor.Configure?.Invoke(descriptor.Instance);
                    return descriptor.Instance;
                }

                if (descriptor.Factory is not null)
                {
                    var service = descriptor.Factory(provider);
                    descriptor.Configure?.Invoke(service);
                    return service;
                }

                if (descriptor.ImplementationType is not null)
                {
                    object? service;

                    if (descriptor.Parameters.Any())
                    {
                        var parameters = descriptor.Parameters.Values.ToArray();
                        try
                        {
                            service = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, parameters!);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    else
                    {
                        service = provider.GetService(descriptor.ImplementationType);
                    }

                    //var instance = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, descriptor.Parameters.Values.ToArray());
                    if (service is not null)
                    {
                        descriptor.Configure?.Invoke(service);
                        return service;
                    }
                }
            }
        }

        return default;
    }

    public static TService GetNamedRequiredService<TService>(this IServiceProvider provider, string name) where TService : notnull
    {
        var service = GetNamedService<TService>(provider, name);

        if (service is not null)
        {
            return service;
        }

        throw new InvalidOperationException($"No named service of type {typeof(TService).FullName} with name '{name}' was registered.");
    }

    public static IReadOnlyDictionary<string, object?> GetMetadata<TService>(this IServiceProvider provider, string name)
        where TService : class
    {
        return provider.GetMetadata(name, typeof(TService));
    }

    public static IReadOnlyDictionary<string, object?> GetMetadata<TService>(this IServiceProvider provider)
        where TService : class
    {
        return provider.GetMetadata(typeof(TService));
    }

    public static IReadOnlyDictionary<string, object?> GetMetadata(this IServiceProvider provider, string name, Type serviceType)
    {
        Guard.NotNullOrEmpty(name, nameof(name));

        Dictionary<string, object?> namedMetadata = new Dictionary<string, object?>();
        Dictionary<string, object?> unnamedMetadata = new Dictionary<string, object?>();

        lock (NamedServices)
        {
            lock (UnnamedServices)
            {
                if (NamedServices.TryGetValue(name, out var services) && services.TryGetValue(serviceType, out var descriptor) && descriptor.Metadata.Any())
                {
                    namedMetadata = descriptor.Metadata;
                }

                if (UnnamedServices.TryGetValue(serviceType, out var unnamedDescriptor))
                {
                    unnamedMetadata = unnamedDescriptor.Metadata;
                }
            }

            return namedMetadata.Merge(unnamedMetadata).AsReadOnly();
        }
    }

    public static IReadOnlyDictionary<string, object?> GetMetadata(this IServiceProvider provider, Type serviceType)
    {
        Guard.NotNull(serviceType, nameof(serviceType));

        lock (UnnamedServices)
        {
            if (UnnamedServices.TryGetValue(serviceType, out var unnamedDescriptor))
            {
                return unnamedDescriptor.Metadata.AsReadOnly();
            }
        }

        return new Dictionary<string, object?>().AsReadOnly();
    }
}
