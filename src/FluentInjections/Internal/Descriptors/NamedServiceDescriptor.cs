// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics.CodeAnalysis;

namespace FluentInjections.Internal.Descriptors;

internal class NamedServiceDescriptor : ServiceDescriptor
{
    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="implementationType"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="implementationType">The <see cref="Type"/> implementing the service.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    internal NamedServiceDescriptor(
        Type serviceType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
        ServiceLifetime lifetime)
        : base(serviceType, null, implementationType, lifetime)
    {
    }

#if false
    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="implementationType"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
    /// <param name="implementationType">The <see cref="Type"/> implementing the service.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    internal NamedServiceDescriptor(
        Type serviceType,
        object? serviceKey,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
        ServiceLifetime lifetime)
        : base(serviceType, serviceKey, lifetime)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="instance"/>
    /// as a <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="instance">The instance implementing the service.</param>
    internal NamedServiceDescriptor(
        Type serviceType,
        object instance)
        : base(serviceType, null, instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="instance"/>
    /// as a <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
    /// <param name="instance">The instance implementing the service.</param>
    internal NamedServiceDescriptor(
        Type serviceType,
        object? serviceKey,
        object instance)
        : base(serviceType, serviceKey, ServiceLifetime.Singleton)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="factory"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="factory">A factory used for creating service instances.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    internal ServiceDescriptor(
        Type serviceType,
        Func<IServiceProvider, object> factory,
        ServiceLifetime lifetime)
        : this(serviceType, serviceKey: null, lifetime)
    {
        ThrowHelper.ThrowIfNull(serviceType);
        ThrowHelper.ThrowIfNull(factory);

        _implementationFactory = factory;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="factory"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
    /// <param name="factory">A factory used for creating service instances.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    internal ServiceDescriptor(
        Type serviceType,
        object? serviceKey,
        Func<IServiceProvider, object?, object> factory,
        ServiceLifetime lifetime)
        : this(serviceType, serviceKey, lifetime)
    {
        ThrowHelper.ThrowIfNull(serviceType);
        ThrowHelper.ThrowIfNull(factory);

        if (serviceKey is null)
        {
            // If the key is null, use the same factory signature as non-keyed descriptor
            Func<IServiceProvider, object> nullKeyedFactory = sp => factory(sp, null);
            _implementationFactory = nullKeyedFactory;
        }
        else
        {
            _implementationFactory = factory;
        }
    }

    private ServiceDescriptor(Type serviceType, object? serviceKey, ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
        ServiceType = serviceType;
        ServiceKey = serviceKey;
    }
    public NamedServiceDescriptor(ServiceDescriptor descriptor, string name) : base(descriptor.
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
#endif
}