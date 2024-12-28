// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Descriptors;

internal class NamedServiceDescriptor : ServiceDescriptor
{
    internal string Name { get; }

    public NamedServiceDescriptor(Type serviceType, object instance, ServiceLifetime lifetime, string name) : base(serviceType, instance, lifetime)
    {
        Name = name;
    }

    public NamedServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, string name) : base(serviceType, implementationType, lifetime)
    {
        Name = name;
    }

    public NamedServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime, string name) : base(serviceType, factory, lifetime)
    {
        Name = name;
    }

    public NamedServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, string name) : base(serviceType, factory)
    {
        Name = name;
    }
}