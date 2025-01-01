// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;
using FluentInjections.Internal.Utils;

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
                        var logger = LoggerUtility.CreateLogger<NetCoreMiddlewareConfigurator<IApplicationBuilder>>();
                        instance.Configure(new NetCoreMiddlewareConfigurator<IApplicationBuilder>(builder, logger));
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
}
