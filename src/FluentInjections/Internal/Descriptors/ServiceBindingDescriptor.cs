// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Descriptors;

public class ServiceBindingDescriptor
{
    public Type BindingType { get; }
    public ServiceLifetime Lifetime { get; set; }

    public Type? ImplementationType { get; set; }
    public object? Instance { get; set; }
    public Func<IServiceProvider, object>? Factory { get; set; }
    public string? Name { get; set; }
    public Action<object>? Configure { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public ServiceConfigurator ServiceConfigurator { get; }

    public ServiceBindingDescriptor(Type bindingType, ServiceConfigurator serviceConfigurator)
    {
        BindingType = bindingType ?? throw new ArgumentNullException(nameof(bindingType));
        ServiceConfigurator = serviceConfigurator ?? throw new ArgumentNullException(nameof(serviceConfigurator));
        Lifetime = ServiceLifetime.Transient;
    }
}

public class ServiceBindingDescriptor<TService>(ServiceConfigurator configurator)
    : ServiceBindingDescriptor(typeof(TService), configurator) where TService : notnull
{ }
