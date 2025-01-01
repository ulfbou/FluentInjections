// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Builder;
using Autofac.Core;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Configurators;

internal class AutofacServiceConfigurator : ServiceConfigurator
{
    private readonly ContainerBuilder _builder;
    internal ContainerBuilder Builder => _builder;

    public AutofacServiceConfigurator(ContainerBuilder builder) : base()
    {
        _builder = builder;
    }

    protected override void Register(ServiceBindingDescriptor descriptor)
    {
        _builder.Register(descriptor);
    }

    private IRegistrationBuilder<TLimit, TActivatorData, TStyle> Register<TLimit, TActivatorData, TStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, ServiceBindingDescriptor descriptor)
    {
        // Handle SimpleActivatorData-based registrations here
        if (descriptor.Lifetime == ServiceLifetime.Singleton)
        {
            builder = builder.SingleInstance();
        }
        else if (descriptor.Lifetime == ServiceLifetime.Scoped)
        {
            builder = builder.InstancePerLifetimeScope();
        }
        else
        {
            builder = builder.InstancePerDependency();
        }

        if (!string.IsNullOrEmpty(descriptor.Name))
        {
            builder = builder.Named(descriptor.Name!, descriptor.BindingType);
        }

        return builder;
    }
}
