// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Builder;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public static class AutofacNamedServiceExtensions
{
    internal static readonly Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>> NamedServices = new();

    public static void Register(this ContainerBuilder builder, ServiceBindingDescriptor descriptor)
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

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

        RegisterDescriptor(builder, descriptor);
    }

    private static void RegisterDescriptor(ContainerBuilder builder, ServiceBindingDescriptor descriptor)
    {
        if (descriptor.Instance is not null)
        {
            builder.RegisterInstance(descriptor.Instance)
                   .As(descriptor.BindingType)
                   .WithLifetime(descriptor.Lifetime)
                   .WithName(descriptor)
                   .WithMetadata(descriptor)
                   .WithConfigure(descriptor);
        }
        else if (descriptor.Factory is not null)
        {
            builder.Register(context =>
            {
                var service = descriptor.Factory(context.Resolve<IServiceProvider>());
                descriptor.Configure?.Invoke(service);
                return service;
            })
            .As(descriptor.BindingType)
            .WithLifetime(descriptor.Lifetime)
            .WithName(descriptor)
            .WithMetadata(descriptor)
            .WithConfigure(descriptor);
        }
        else if (descriptor.ImplementationType is not null)
        {
            if (descriptor.Parameters.Any())
            {
                builder.Register(context =>
                {
                    var service = ActivatorUtilities.CreateInstance(context.Resolve<IServiceProvider>(), descriptor.ImplementationType, descriptor.Parameters.Values.ToArray());
                    descriptor.Configure?.Invoke(service);
                    return service;
                })
                .As(descriptor.BindingType)
                .WithLifetime(descriptor.Lifetime)
                .WithName(descriptor)
                .WithMetadata(descriptor)
                .WithConfigure(descriptor);
            }
            else
            {
                if (descriptor.BindingType == descriptor.ImplementationType)
                {
                    builder.RegisterType(descriptor.ImplementationType)
                           .As(descriptor.BindingType)
                           .WithLifetime(descriptor.Lifetime)
                           .WithName(descriptor)
                           .WithMetadata(descriptor)
                           .WithConfigure(descriptor);
                }
                else
                {
                    builder.RegisterType(descriptor.ImplementationType)
                           .As(descriptor.BindingType)
                           .WithLifetime(descriptor.Lifetime)
                           .WithName(descriptor)
                           .WithMetadata(descriptor)
                           .WithConfigure(descriptor);

                    builder.RegisterType(descriptor.ImplementationType)
                           .AsSelf()
                           .WithLifetime(descriptor.Lifetime)
                           .WithMetadata(descriptor)
                           .WithConfigure(descriptor);
                }
            }
        }
        else
        {
            throw new InvalidOperationException("ServiceBindingDescriptor must have an Instance, Factory, or ImplementationType defined.");
        }
    }

    internal static IRegistrationBuilder<TLimit, TActivatorData, TStyle> WithLifetime<TLimit, TActivatorData, TStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, ServiceLifetime lifetime)
    {
        return lifetime switch
        {
            ServiceLifetime.Singleton => builder.SingleInstance(),
            ServiceLifetime.Scoped => builder.InstancePerLifetimeScope(),
            ServiceLifetime.Transient => builder.InstancePerDependency(),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null)
        };
    }

    internal static IRegistrationBuilder<TLimit, TActivatorData, TStyle> WithName<TLimit, TActivatorData, TStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, ServiceBindingDescriptor descriptor)
    {
        if (!string.IsNullOrEmpty(descriptor.Name))
        {
            registration.Named(descriptor.Name, descriptor.BindingType);
        }

        return registration;
    }

    internal static IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> WithMetadata<T>(
        this IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> registration, ServiceBindingDescriptor descriptor)
    {
        return registration.WithMetadata(descriptor.Metadata.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)));
    }

    internal static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> WithParameters<T>(
        this IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration, ServiceBindingDescriptor descriptor)
    {
        foreach (var parameter in descriptor.Parameters)
        {
            registration.WithParameter(new NamedParameter(parameter.Key, parameter.Value));
        }

        return registration;
    }

    internal static IRegistrationBuilder<TLimit, TActivatorData, TStyle> WithConfigure<TLimit, TActivatorData, TStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, ServiceBindingDescriptor descriptor)
    {
        registration.OnActivated(e =>
        {
            Guard.NotNull(e.Instance, nameof(e.Instance));
            descriptor.Configure?.Invoke(e.Instance!);
        });

        return registration;
    }

    /// <summary>
    /// Gets the named service of the specified type.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="context">The component context.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The named service instance.</returns>
    public static TService? GetNamedService<TService>(this IComponentContext context, string name) where TService : notnull
    {
        return (TService?)GetNamedService(context, typeof(TService), name);
    }

    /// <summary>
    /// Gets the named service of the specified type.
    /// </summary>
    /// <param name="context">The component context.</param>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The named service instance.</returns>
    public static object? GetNamedService(this IComponentContext context, Type serviceType, string name)
    {
        Guard.NotNull(context, nameof(context));
        Guard.NotNullOrEmpty(name, nameof(name));
        return context.TryResolveNamed(name, serviceType, out var service) ? service : null;
    }

    /// <summary>
    /// Gets the named service of the specified type.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="context">The component context.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The named service instance.</returns>
    public static TService GetNamedRequiredService<TService>(this IComponentContext context, string name) where TService : notnull
    {
        return GetNamedService(context, typeof(TService), name) switch
        {
            TService service => service,
            _ => throw new InvalidOperationException($"No named service of type {typeof(TService).FullName} with name '{name}' was registered.")
        };
    }

    /// <summary>
    /// Gets the metadata of the named service of the specified type.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="context">The component context.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The metadata of the named service.</returns>
    public static IReadOnlyDictionary<string, object> GetMetadata<TService>(this IComponentContext context, string name) where TService : notnull
    {
        return GetMetadata(context, typeof(TService), name);
    }

    /// <summary>
    /// Gets the metadata of the named service of the specified type.
    /// </summary>
    /// <param name="context">The component context.</param>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The metadata of the named service.</returns>
    private static IReadOnlyDictionary<string, object> GetMetadata(IComponentContext context, Type type, string name)
    {
        Guard.NotNull(context, nameof(context));
        Guard.NotNullOrEmpty(name, nameof(name));

        if (NamedServices.TryGetValue(name, out var services) && services.TryGetValue(type, out var descriptor))
        {
            return (descriptor.Metadata ?? new Dictionary<string, object>()).AsReadOnly();
        }

        return Enumerable.Empty<KeyValuePair<string, object>>().ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
    }
}