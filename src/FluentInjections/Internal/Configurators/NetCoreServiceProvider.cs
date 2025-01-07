// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentInjections.Internal.Configurators;

public class NetCoreServiceProvider :
    IServiceProvider,
    ISupportRequiredService,
    IKeyedServiceProvider,
    IServiceProviderIsService,
    IServiceProviderIsKeyedService,
    IDisposable,
    IAsyncDisposable
{
    protected readonly IServiceProvider _provider;
    protected readonly IDictionary<string, ServiceDescriptor> _keyedServiceDescriptors;
    protected readonly IDictionary<string, Dictionary<Type, ServiceBindingDescriptor>> _namedServices;

    public NetCoreServiceProvider(IServiceProvider serviceProvider, IDictionary<string, ServiceDescriptor> keyedServiceDescriptors)
    {
        _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _keyedServiceDescriptors = keyedServiceDescriptors ?? throw new ArgumentNullException(nameof(keyedServiceDescriptors));
        _namedServices = new Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>>();
    }

    public NetCoreServiceProvider(IServiceProvider serviceProvider, Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>> namedServices)
    {
        _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _keyedServiceDescriptors = new Dictionary<string, ServiceDescriptor>();
        _namedServices = namedServices ?? throw new ArgumentNullException(nameof(namedServices));
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        Guard.NotNull(serviceType, nameof(serviceType));
        Guard.NotNull(serviceKey, nameof(serviceKey));
        Guard.IsType<string>(serviceKey!);

        lock (_namedServices)
        {
            string name = (serviceKey as string)!;

            if (_namedServices.TryGetValue(name!, out var services) && services.TryGetValue(serviceType, out var descriptor))
            {
                if (descriptor.Instance is not null)
                {
                    descriptor.Configure?.Invoke(descriptor.Instance);
                    return descriptor.Instance;
                }

                if (descriptor.Factory is not null)
                {
                    var service = descriptor.Factory(_provider);
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
                            service = ActivatorUtilities.CreateInstance(_provider, descriptor.ImplementationType, parameters);
                        }
                        catch
                        {
                            return default;
                        }
                    }
                    else
                    {
                        service = _provider.GetService(descriptor.ImplementationType);
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

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        return GetKeyedService(serviceType, serviceKey) ?? throw new InvalidOperationException($"Service type '{serviceType.Name}' with service key '{serviceKey}' not found.");
    }

    public object GetRequiredService(Type serviceType) => _provider.GetRequiredService(serviceType);
    public object? GetService(Type serviceType) => _provider.GetService(serviceType);

    public ValueTask DisposeAsync()
    {
        if (_provider is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        Dispose();
        return default;
    }

    public bool IsKeyedService(Type serviceType, object? serviceKey)
    {
        Guard.NotNull(serviceType, nameof(serviceType));
        Guard.NotNull(serviceKey, nameof(serviceKey));
        Guard.IsType<string>(serviceKey!);

        lock (_namedServices)
        {
            string name = (serviceKey as string)!;

            if (_namedServices.TryGetValue(name!, out var services) && services.TryGetValue(serviceType, out _))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsService(Type serviceType) => throw new NotImplementedException();

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
#if false
    public object? GetKeyedService(string key, Type serviceType)
    {
        if (_keyedServiceDescriptors.TryGetValue(key, out var descriptor))
        {
            return descriptor switch
            {
                { ImplementationType: not null } => _serviceProvider.GetService(descriptor.ImplementationType),
                { ImplementationFactory: not null } => descriptor.ImplementationFactory(_serviceProvider),
                { ImplementationInstance: not null } => descriptor.ImplementationInstance,
                _ => null
            };
        }

        throw new InvalidOperationException($"No service registered with the key '{key}'");
    }

    public TService? GetKeyedService<TService>(string key) where TService : notnull
    {
        return (TService?)GetKeyedService(key, typeof(TService));
    }

    public TService GetRequiredKeyedService<TService>(string key) where TService : notnull
    {
        return GetKeyedService<TService>(key)
            ?? throw new InvalidOperationException($"Service '{key}' not found.");
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        if (serviceKey is string key)
        {
            return GetKeyedService(key, serviceType);
        }
        throw new InvalidOperationException("Service key must be a string.");
    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        return GetKeyedService(serviceType, serviceKey)
            ?? throw new InvalidOperationException($"Service '{serviceKey}' not found.");
    }
#endif
}
