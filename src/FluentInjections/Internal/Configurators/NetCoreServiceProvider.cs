// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentInjections.Internal.Configurators;

public class NetCoreServiceProvider : IServiceProvider, IKeyedServiceProvider
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IDictionary<string, ServiceDescriptor> _keyedServiceDescriptors;

    public NetCoreServiceProvider(IServiceProvider serviceProvider, IDictionary<string, ServiceDescriptor> keyedServiceDescriptors)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _keyedServiceDescriptors = keyedServiceDescriptors ?? throw new ArgumentNullException(nameof(keyedServiceDescriptors));
    }

    public object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }

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
}
