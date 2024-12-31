// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Builder;
using Autofac.Core;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Configurators;

public class AutofacServiceConfigurator : ServiceConfigurator
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
        //IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration = null!;

        //if (descriptor.ImplementationType is not null)
        //{
        //    registration = Register(
        //        _builder.RegisterType(descriptor.ImplementationType).As(descriptor.BindingType),
        //        descriptor);
        //}

        //if (descriptor.Instance is not null)
        //{
        //    var instanceRegistration = _builder.RegisterInstance(descriptor.Instance!).As(descriptor.BindingType);

        //    instanceRegistration = Register(instanceRegistration, descriptor);

        //    if (descriptor.Configure is not null)
        //    {
        //        instanceRegistration.OnActivating(e => descriptor.Configure(e.Instance!));
        //    }

        //    return;
        //}

        //if (descriptor.Factory is not null)
        //{
        //    var factoryRegistration = _builder.Register(c => descriptor.Factory!(c.Resolve<IServiceProvider>())).As(descriptor.BindingType);

        //    if (descriptor.Configure is not null)
        //    {
        //        factoryRegistration.OnActivating(e => descriptor.Configure(e.Instance!));
        //    }

        //    return;
        //}

        //if (registration is null)
        //{
        //    registration = Register(_builder.RegisterType(descriptor.BindingType).AsSelf(), descriptor);
        //}

        //if (descriptor.Parameters.Any())
        //{
        //    if (!registration.IsReflectionData())
        //    {
        //        throw new InvalidOperationException("Parameters are only supported for reflection-based registrations.");
        //    }

        //    var parameters = descriptor.Parameters.Select(parameter => new ResolvedParameter((pi, ctx) => pi.Name == parameter.Key, (pi, ctx) => parameter.Value)).ToList();
        //    registration.WithParameters(parameters);
        //}

        //if (descriptor.Configure is not null)
        //{
        //    registration.OnActivating(e => descriptor.Configure(e.Instance!));
        //}
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
