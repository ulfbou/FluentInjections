// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;

using FluentInjections;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Reflection;

namespace FluentInjections
{
    internal static class DependencyInjection
    {
        internal static readonly object LockObject = new object();
        internal static IServiceCollection Services { get; private set; } = new ServiceCollection();
        internal static ContainerBuilder Builder { get; private set; } = new ContainerBuilder();
        internal static AutofacServiceProvider ServiceProvider { get; private set; }
        internal static IContainer Container { get; private set; }

        static DependencyInjection()
        {
            lock (LockObject)
            {
                Container = default!;
                ServiceProvider = default!;
            }
        }

        internal static void Initialize()
        {
            lock (LockObject)
            {
                Services = new ServiceCollection();
                Builder = new ContainerBuilder();
                Container = default!;
                ServiceProvider = default!;
            }
        }

        internal static void AddFluentInjections(IServiceCollection services, params Assembly[]? assemblies)
        {
            lock (LockObject)
            {
                Services = services;
                var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

                Builder.Populate(Services);
                Builder.RegisterModule(new FluentInjectionsModule(Services, targetAssemblies));
                Container = Builder.Build();
                ServiceProvider = new AutofacServiceProvider(Container);

                var serviceProviders = services.Where(s => s.ServiceType == typeof(IServiceProvider)).ToList();
                foreach (var serviceProvider in serviceProviders)
                {
                    services.Remove(serviceProvider);
                }

                services.AddSingleton<IServiceProvider>(ServiceProvider);
                services.AddSingleton(ServiceProvider);
                services.AddSingleton<IContainer>(Container);
            }
        }

        internal static void UseFluentInjections(IApplicationBuilder builder, params Assembly[]? assemblies)
        {
            lock (LockObject)
            {
                var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
                var sp = builder.ApplicationServices;
                var services = sp.GetRequiredService<IServiceCollection>();
                var registry = sp.GetService<IModuleRegistry>() ?? new ModuleRegistry(services);

                foreach (var assembly in targetAssemblies)
                {
                    foreach (var type in assembly.GetTypes().Where(type => typeof(IMiddlewareModule).IsAssignableFrom(type) && !type.IsAbstract))
                    {
                        var instance = GetInstance<IMiddlewareModule, IMiddlewareConfigurator>(type, sp);
                        if (instance != null)
                        {
                            instance.Configure(new MiddlewareConfigurator(DependencyInjection.Builder));
                            registry.Register<IMiddlewareConfigurator>(typeof(IMiddlewareModule), instance);
                        }
                    }
                }
            }
        }

        private static IConfigurableModule<TConfigurator>? GetInstance<TModule, TConfigurator>(Type type, IServiceProvider? sp = null)
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
}
