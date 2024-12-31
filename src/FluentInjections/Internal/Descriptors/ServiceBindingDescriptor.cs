// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

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

    public ServiceBindingDescriptor(Type bindingType)
    {
        BindingType = bindingType ?? throw new ArgumentNullException(nameof(bindingType));
        Lifetime = ServiceLifetime.Transient;
    }
}

public class ServiceBindingDescriptor<TService>() : ServiceBindingDescriptor(typeof(TService)) where TService : notnull { }
