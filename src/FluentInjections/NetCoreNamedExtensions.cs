// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections;
using FluentInjections.Extensions;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Helpers;
using FluentInjections.Internal.ServiceProvider;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Reflection;

namespace FluentInjections;

public static class NetCoreNamedExtensions
{
    internal static object LockObject = new();
    internal static readonly Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>> NamedServices = new();
    internal static readonly Dictionary<Type, ServiceBindingDescriptor> UnnamedServices = new();

    internal static IServiceCollection? Services { get; set; }
    internal static Assembly[]? TargetAssemblies { get; set; }
    internal static NetCoreServiceConfigurator? ServiceConfigurator { get; set; }
    internal static NetCoreMiddlewareConfigurator? MiddlewareConfigurator { get; set; }
    internal static IApplicationBuilder? AppBuilder { get; set; }
    internal static NetCoreServiceProvider? ServiceProvider { get; set; }
    public static IHostBuilder? Host { get; set; }

    #region Module Discovery
    /// <summary>
    /// Adds FluentInjections to the service collection by scanning the specified assemblies for <see cref="IModule{IServiceConfigurator}"/> implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for FluentInjections.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown if FluentInjections has already been initialized.</exception>
    public static IServiceCollection AddFluentInjections(this IServiceCollection services, IServiceConfigurator? configurator = null, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            if (Services is not null)
            {
                Debug.WriteLine("FluentInjections has already been configured.");
                return services;
            }

            Debug.WriteLine("Configuring FluentInjections services.");

            Services = services;
            TargetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            var logger = LoggerUtility.CreateLogger<NetCoreServiceConfigurator>();
            ServiceConfigurator = new NetCoreServiceConfigurator(services, logger);


            var modules = GetModules<IServiceConfigurator>(TargetAssemblies);
            var count = modules.Count();

            // Register service modules with PLINQ
            foreach (var module in modules)
            {
                RegisterHelper.RegisterModule<IServiceConfigurator>(module.Module, typeof(IServiceModule), ServiceConfigurator);
            }

            ServiceConfigurator.Register();

            return services;
        }
    }

    /// <summary>
    /// Configures the host builder to use FluentInjections as the service provider.
    /// </summary>
    public static ConfigureHostBuilder AddFluentInjectionsServiceProvider(this ConfigureHostBuilder hostBuilder)
    {
        lock (LockObject)
        {
            if (ServiceConfigurator is not null)
            {
                hostBuilder.ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<NetCoreServiceConfigurator>(ServiceConfigurator);
                    });
            }

            hostBuilder.UseServiceProviderFactory(new FluentInjectionsServiceProviderFactory());

            return hostBuilder;
        }
    }

    public static WebApplicationBuilder AddFluentInjections(this WebApplicationBuilder builder, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            if (ServiceConfigurator is null)
            {
                throw new InvalidOperationException("FluentInjections has not been configured. Ensure that AddFluentInjections has been called before calling UseFluentInjections.");
            }

            Debug.WriteLine("Configuring FluentInjections services.");

            Services = builder.Services;
            TargetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            var logger = LoggerUtility.CreateLogger<NetCoreServiceConfigurator>();
            ServiceConfigurator = new NetCoreServiceConfigurator(builder.Services, logger);
            var modules = GetModules<IServiceConfigurator>(TargetAssemblies);

            // Register service modules with PLINQ
            foreach (var module in modules)
            {
                RegisterHelper.RegisterModule<IServiceConfigurator>(module.Module, typeof(IServiceModule), ServiceConfigurator);
            }

            ServiceConfigurator.Register();

            Host = builder.Host;

            Host.UseServiceProviderFactory(new FluentInjectionsServiceProviderFactory());

            return builder;
        }
    }

    /// <summary>
    /// Adds FluentInjections to the service collection by scanning the specified assemblies for <see cref="IModule{IMiddlewareConfigurator}"/> implementations.
    /// </summary>
    /// <param name="application">The application builder.</param>
    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            if (ServiceConfigurator is null)
            {
                throw new InvalidOperationException("FluentInjections has not been configured. Ensure that AddFluentInjections has been called before calling UseFluentInjections.");
            }

            Debug.WriteLine("Configuring FluentInjections middleware");

            AppBuilder = app;
            TargetAssemblies = assemblies?.Length > 0 ? assemblies : TargetAssemblies ?? AppDomain.CurrentDomain.GetAssemblies();
            ServiceProvider = ServiceConfigurator.BuildServiceProvider(Services);
            app.ApplicationServices = ServiceProvider;
            MiddlewareConfigurator = new NetCoreMiddlewareConfigurator(app, ServiceProvider, LoggerUtility.CreateLogger<NetCoreMiddlewareConfigurator>());

            // Register middleware modules with PLINQ
            var modules = GetModules<IMiddlewareConfigurator>(TargetAssemblies);

            foreach (var module in modules)
            {
                RegisterHelper.RegisterModule<IMiddlewareConfigurator>(module.Module, typeof(IMiddlewareModule), MiddlewareConfigurator);
            }

            MiddlewareConfigurator.Register();

            return app;
        }
    }

    private static IEnumerable<ModuleType> GetModules<TConfigurator>(Assembly[] targetAssemblies)
        where TConfigurator : class, IConfigurator
    {
        return targetAssemblies.SelectMany(a => a.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.IsPublic))
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>) &&
                i.GetGenericArguments()[0] == typeof(TConfigurator)))
            .Select(t =>
            {
                var matchingInterface = t.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>) &&
                    i.GetGenericArguments()[0] == typeof(TConfigurator));

                if (matchingInterface == null)
                {
                    // Log a warning or throw a more specific exception if this is unexpected.
                    // For now, we'll just skip this type.
                    Debug.WriteLine($"Warning: Type {t.FullName} implements IConfigurableModule but not with the expected TConfigurator type.");
                    throw new InvalidOperationException($"Module {t.FullName} does not implement IModule<{typeof(TConfigurator).FullName}>.");
                }

                return new ModuleType(t, matchingInterface);
            });
    }
    #endregion

    #region Register
    // Register service binding descriptor
    internal static void Register(this IServiceCollection services, ServiceBindingDescriptor descriptor)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(descriptor, nameof(descriptor));

        lock (NamedServices)
        {
            if (string.IsNullOrEmpty(descriptor.Name))
            {
                UnnamedServices[descriptor.BindingType] = descriptor;
            }
            else
            {
                if (!NamedServices.ContainsKey(descriptor.Name))
                {
                    NamedServices[descriptor.Name] = new Dictionary<Type, ServiceBindingDescriptor>();
                }

                NamedServices[descriptor.Name][descriptor.BindingType] = descriptor;
            }
        }

        AddServiceDescriptor(services, descriptor);
    }

    private static void AddServiceDescriptor(IServiceCollection services, ServiceBindingDescriptor descriptor)
    {
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
        else if (descriptor.ImplementationType is null)
        {
            throw new InvalidOperationException("ServiceBindingDescriptor must have an Instance, Factory, or ImplementationType defined.");
        }
        else if (descriptor.Parameters is null || descriptor.Parameters.Count == 0)
        {
            if (descriptor.BindingType != descriptor.ImplementationType && !services.Any(sd => sd.ServiceType == descriptor.BindingType))
            {
                services.Add(new ServiceDescriptor(descriptor.BindingType, descriptor.ImplementationType!, descriptor.Lifetime));
            }

            services.Add(new ServiceDescriptor(descriptor.ImplementationType, descriptor.ImplementationType, descriptor.Lifetime));
        }
        else
        {
            services.Add(new ServiceDescriptor(descriptor.BindingType, provider =>
            {
                var service = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, descriptor.Parameters.Values.ToArray().Where(p => p is not null));
                descriptor.Configure?.Invoke(service);
                return service;
            }, descriptor.Lifetime));
        }
    }
    #endregion

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
                            service = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType, parameters!);
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

    public static IReadOnlyDictionary<string, object?> GetMetadata<TService>(this IServiceProvider provider, string name)
        where TService : class
    {
        return provider.GetMetadata(name, typeof(TService));
    }

    public static IReadOnlyDictionary<string, object?> GetMetadata<TService>(this IServiceProvider provider)
        where TService : class
    {
        return provider.GetMetadata(typeof(TService));
    }

    public static IReadOnlyDictionary<string, object?> GetMetadata(this IServiceProvider provider, string name, Type serviceType)
    {
        Guard.NotNullOrEmpty(name, nameof(name));

        Dictionary<string, object?> namedMetadata = new Dictionary<string, object?>();
        Dictionary<string, object?> unnamedMetadata = new Dictionary<string, object?>();

        lock (NamedServices)
        {
            lock (UnnamedServices)
            {
                if (NamedServices.TryGetValue(name, out var services) && services.TryGetValue(serviceType, out var descriptor) && descriptor.Metadata.Any())
                {
                    namedMetadata = descriptor.Metadata;
                }

                if (UnnamedServices.TryGetValue(serviceType, out var unnamedDescriptor))
                {
                    unnamedMetadata = unnamedDescriptor.Metadata;
                }
            }

            return namedMetadata.Merge(unnamedMetadata).AsReadOnly();
        }
    }

    public static IReadOnlyDictionary<string, object?> GetMetadata(this IServiceProvider provider, Type serviceType)
    {
        Guard.NotNull(serviceType, nameof(serviceType));

        lock (UnnamedServices)
        {
            if (UnnamedServices.TryGetValue(serviceType, out var unnamedDescriptor))
            {
                return unnamedDescriptor.Metadata.AsReadOnly();
            }
        }

        return new Dictionary<string, object?>().AsReadOnly();
    }
}
