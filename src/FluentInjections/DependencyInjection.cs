// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;

using FluentInjections;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Registries;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection;

namespace FluentInjections;

internal static class DependencyInjection
{
    internal static readonly object LockObject = new object();
    internal static IServiceCollection Services { get; private set; } = new ServiceCollection();
    internal static IServiceProvider ServiceProvider { get; private set; }
    internal static Mock<ILogger> MockLogger { get; private set; }

    private static bool _initialized;

    static DependencyInjection()
    {
        lock (LockObject)
        {
            ServiceProvider = default!;
            MockLogger = new Mock<ILogger>();
            _initialized = false;
        }
    }

    internal static void Initialize()
    {
        lock (LockObject)
        {
            Services = new ServiceCollection();
            ServiceProvider = default!;
            MockLogger = new Mock<ILogger>();
            _initialized = false;
        }
    }

    internal static void AddFluentInjections<TBuilder>(IServiceCollection services, TBuilder builder, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("FluentInjections has already been initialized.");
            }

            Services = services;
            var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            var module = new FluentInjectionsNetCoreModule<TBuilder>(Services, builder, targetAssemblies);

            module.Load();

            var serviceProviders = Services.Where(s => s.ServiceType == typeof(IServiceProvider)).ToList();
            foreach (var serviceProvider in serviceProviders)
            {
                Services.Remove(serviceProvider);
            }

            Services.AddSingleton<IServiceProvider>(ServiceProvider);
            Services.AddSingleton(ServiceProvider);

            _initialized = true;
        }
    }

    internal static void AddFluentInjections(ContainerBuilder builder, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("FluentInjections has already been initialized.");
            }

            var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            builder.RegisterModule(new FluentInjectionsAutofacModule(targetAssemblies));
            var Container = builder.Build();
            ServiceProvider = new AutofacServiceProvider(Container);
            var serviceProviders = Services.Where(s => s.ServiceType == typeof(IServiceProvider)).ToList();

            foreach (var serviceProvider in serviceProviders)
            {
                Services.Remove(serviceProvider);
            }
            Services.AddSingleton<IServiceProvider>(ServiceProvider);
            Services.AddSingleton(ServiceProvider);
            Services.AddSingleton<IContainer>(Container);
            _initialized = true;
        }
    }

    internal static void UseFluentInjections(IApplicationBuilder builder, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("FluentInjections has not been initialized. Call AddFluentInjections first.");
            }

            var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            var sp = builder.ApplicationServices;
            var registry = sp.GetService<IModuleRegistry>() ?? new ModuleRegistry(Services);

            foreach (var assembly in targetAssemblies)
            {
                foreach (var type in assembly.GetTypes().Where(type => typeof(IMiddlewareModule).IsAssignableFrom(type) && !type.IsAbstract))
                {
                    var instance = GetInstance<IMiddlewareModule, IMiddlewareConfigurator>(type, sp);
                    if (instance != null)
                    {
                        instance.Configure(new NetCoreMiddlewareConfigurator<IApplicationBuilder>(builder));
                        registry.Register<IMiddlewareConfigurator>(typeof(IMiddlewareModule), instance);
                    }
                }
            }
        }
    }

    internal static IConfigurableModule<TConfigurator>? GetInstance<TModule, TConfigurator>(Type type, IServiceProvider? sp = null)
        where TModule : IConfigurableModule<TConfigurator>
        where TConfigurator : IConfigurator
    {
        if (!typeof(TModule).IsAssignableFrom(type)) return null;

        var instance = sp?.GetService(type) as IConfigurableModule<TConfigurator>;
        if (instance != null)
        {
            return instance;
        }

        try
        {
            instance = Activator.CreateInstance(type) as IConfigurableModule<TConfigurator>;
            return instance;
        }
        catch (Exception ex)
        {
            var logger = sp?.GetService<ILogger>();
            logger?.LogError(ex, $"Could not create an instance of {type.Name}.");
            return null;
        }
    }

    private static readonly Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>> NamedServices = new();

    internal static void Register(this IServiceCollection services, ServiceBindingDescriptor descriptor)
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        if (!string.IsNullOrEmpty(descriptor.Name))
        {
            lock (NamedServices)
            {
                if (!NamedServices.ContainsKey(descriptor.Name))
                {
                    NamedServices[descriptor.Name] = new Dictionary<Type, ServiceBindingDescriptor>();
                }

                NamedServices[descriptor.Name][descriptor.BindingType] = descriptor;
            }
        }

        if (descriptor.Instance is not null)
        {
            services.Add(new ServiceDescriptor(descriptor.BindingType, provider =>
            {
                descriptor.Configure?.Invoke(descriptor.Instance);
                return descriptor.Instance;
            }, ServiceLifetime.Singleton));
        }
        else if (descriptor.Factory is not null)
        {
            services.Add(new ServiceDescriptor(descriptor.BindingType, provider =>
            {
                var service = descriptor.Factory(provider);
                descriptor.Configure?.Invoke(service);
                return service;
            }, descriptor.Lifetime));
        }
        else if (descriptor.ImplementationType is not null)
        {
            if (descriptor.Parameters.Any())
            {
                services.Add(new ServiceDescriptor(descriptor.BindingType, provider =>
                {
                    var service = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, descriptor.Parameters.Values.ToArray());
                    descriptor.Configure?.Invoke(service); return service;
                }, descriptor.Lifetime));
            }
            else
            {
                if (descriptor.BindingType != descriptor.ImplementationType)
                {
                    if (!services.Any(sd => sd.ServiceType == descriptor.BindingType))
                    {
                        services.Add(new ServiceDescriptor(descriptor.BindingType, descriptor.ImplementationType, descriptor.Lifetime));
                    }
                }

                services.Add(new ServiceDescriptor(descriptor.ImplementationType, descriptor.ImplementationType, descriptor.Lifetime));
            }
        }
        else
        {
            throw new InvalidOperationException("ServiceBindingDescriptor must have an Instance, Factory, or ImplementationType defined.");
        }
    }

    public static TService? GetNamedService<TService>(this IServiceProvider provider, string name) where TService : notnull
    {
        var service = GetNamedService(provider, typeof(TService), name);

        if (service is not null)
        {
            return (TService)service;
        }

        return default;
    }

    public static object? GetNamedService(this IServiceProvider provider, Type serviceType, string name)
    {
        Guard.NotNull(provider, nameof(provider));
        Guard.NotNullOrEmpty(name, nameof(name));

        lock (NamedServices)
        {
            if (NamedServices.TryGetValue(name, out var services) && services.TryGetValue(serviceType, out var descriptor))
            {
                if (descriptor.Instance is not null)
                {
                    descriptor.Configure?.Invoke(descriptor.Instance);
                    return descriptor.Instance;
                }

                if (descriptor.Factory is not null)
                {
                    var service = descriptor.Factory(provider);
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
                            service = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, parameters);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    else
                    {
                        service = provider.GetService(descriptor.ImplementationType);
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

    public static TService GetNamedRequiredService<TService>(this IServiceProvider provider, string name) where TService : notnull
    {
        var service = GetNamedService<TService>(provider, name);

        if (service is not null)
        {
            return service;
        }

        throw new InvalidOperationException($"No named service of type {typeof(TService).FullName} with name '{name}' was registered.");
    }


    public static IReadOnlyDictionary<string, object> GetMetadata(this IServiceProvider provider, string name, Type serviceType)
    {
        Guard.NotNullOrEmpty(name, nameof(name));

        lock (NamedServices)
        {
            if (NamedServices.TryGetValue(name, out var services) && services.TryGetValue(serviceType, out var descriptor))
            {
                return new Dictionary<string, object>(descriptor.Metadata);
            }
        }

        throw new InvalidOperationException($"No metadata found for service of type {serviceType.FullName} with name '{name}'.");
    }

    public static void Register(this ContainerBuilder builder, ServiceBindingDescriptor descriptor)
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

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
                    // Register BindingType to ImplementationType
                    builder.RegisterType(descriptor.ImplementationType)
                           .As(descriptor.BindingType)
                           .WithLifetime(descriptor.Lifetime)
                           .WithName(descriptor)
                           .WithMetadata(descriptor)
                           .WithConfigure(descriptor);

                    // Register ImplementationType to itself
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

    public static TService? GetNamedService<TService>(this IComponentContext context, string name) where TService : notnull
    {
        return (TService?)GetNamedService(context, typeof(TService), name);
    }

    public static object? GetNamedService(this IComponentContext context, Type serviceType, string name)
    {
        Guard.NotNull(context, nameof(context));
        Guard.NotNullOrEmpty(name, nameof(name));

        return context.TryResolveNamed(name, serviceType, out var service) ? service : null;
    }

    public static TService GetNamedRequiredService<TService>(this IComponentContext context, string name) where TService : notnull
    {
        return GetNamedService(context, typeof(TService), name) switch
        {
            TService service => service,
            _ => throw new InvalidOperationException($"No named service of type {typeof(TService).FullName} with name '{name}' was registered.")
        };
    }

    public static IReadOnlyDictionary<string, object> GetMetadata<TService>(this IComponentContext context, string name) where TService : notnull
    {
        return GetMetadata(context, typeof(TService), name);
    }

    public static IReadOnlyDictionary<string, object> GetMetadata(this IComponentContext context, Type serviceType, string name)
    {
        Guard.NotNull(context, nameof(context));
        Guard.NotNullOrEmpty(name, nameof(name));

        if (context.ComponentRegistry.TryGetRegistration(
                new Autofac.Core.KeyedService(name, serviceType),
                out var registration))
        {
            return registration?.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly()!;
        }

        throw new InvalidOperationException($"No metadata found for service of type {serviceType.FullName} with name '{name}'.");
    }
}
