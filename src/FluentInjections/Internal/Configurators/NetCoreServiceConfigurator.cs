// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentInjections.Internal.Configurators;

internal class NetCoreServiceConfigurator : ServiceConfigurator, IServiceConfigurator
{
    private readonly IDictionary<string, ServiceDescriptor> _keyedServiceDescriptors = new Dictionary<string, ServiceDescriptor>();
    private readonly IServiceCollection _services;
    internal IServiceCollection DependencyBuilder => _services;

    public NetCoreServiceConfigurator(IServiceCollection services, ILogger<NetCoreServiceConfigurator> logger) : base(logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    protected override void Register(ServiceBindingDescriptor bindingDescriptor)
    {
        _services.Register(bindingDescriptor);
    }

    private ServiceDescriptor Register(ServiceDescriptor descriptor, ServiceBindingDescriptor bindingDescriptor)
    {
        Guard.NotNull(descriptor, nameof(descriptor));
        Guard.NotNull(bindingDescriptor, nameof(bindingDescriptor));

        // Handle additional configurations if needed
        if (descriptor.ImplementationType is not null)
        {
            bindingDescriptor.Configure?.Invoke(bindingDescriptor.ImplementationType!);
        }

        return descriptor;
    }

    private void AddServiceDescriptor(ServiceBindingDescriptor bindingDescriptor, ServiceDescriptor descriptor)
    {
        if (bindingDescriptor.Name is not null)
        {
            _keyedServiceDescriptors[bindingDescriptor.Name] = descriptor;
        }
        else
        {
            _services.Add(descriptor);
        }
    }

    internal IServiceProvider BuildServiceProvider()
    {
        var serviceProvider = _services.BuildServiceProvider();
        return new NetCoreServiceProvider(serviceProvider, _keyedServiceDescriptors);
    }

    internal TService? GetKeyedService<TService>(IServiceProvider provider, string key) where TService : notnull
    {
        var customProvider = provider as NetCoreServiceProvider ?? throw new InvalidOperationException("Invalid service provider.");
        return customProvider.GetKeyedService<TService>(key);
    }

    internal TService GetRequiredKeyedService<TService>(IServiceProvider provider, string key) where TService : notnull
    {
        var customProvider = provider as NetCoreServiceProvider ?? throw new InvalidOperationException("Invalid service provider.");
        return customProvider.GetRequiredKeyedService<TService>(key)
            ?? throw new InvalidOperationException($"Service '{key}' not found.");
    }
}
