// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Builder;

using FluentInjections;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public static class NetCoreNamedServiceExtensions
{
    internal static readonly Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>> NamedServices = new();

    internal static void Register(this IServiceCollection services, ServiceBindingDescriptor descriptor)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(descriptor, nameof(descriptor));

        lock (NamedServices)
        {
            if (!string.IsNullOrEmpty(descriptor.Name))
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
                            service = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, parameters);
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


    public static IReadOnlyDictionary<string, object> GetMetadata(this IServiceProvider provider, string name, Type serviceType)
    {
        Guard.NotNullOrEmpty(name, nameof(name));

        lock (NamedServices)
        {
            if (NamedServices.TryGetValue(name, out var services) && services.TryGetValue(serviceType, out var descriptor))
            {
                return new Dictionary<string, object>(descriptor.Metadata);
            }
        }

        throw new InvalidOperationException($"No metadata found for service of type {serviceType.FullName} with name '{name}'.");
    }
}
