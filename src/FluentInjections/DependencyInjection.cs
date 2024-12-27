using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Registries;
using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Reflection;

namespace FluentInjections;

public static class DependencyInjection
{
    private static readonly object LockObject = new();

    internal static IServiceCollection Services { get; private set; }
    internal static ContainerBuilder Builder { get; private set; }
    internal static AutofacServiceProvider ServiceProvider { get; private set; }
    internal static IContainer Container { get; private set; }

    // Static constructor to initialize the container builder, service provider, and container
    static DependencyInjection()
    {
        Services = default!;
        Builder = new ContainerBuilder();
        Container = default!;
        ServiceProvider = default!;
    }

    internal static void InitializeTests()
    {
        lock (LockObject)
        {
            var builder = WebApplication.CreateBuilder();
            Services = builder.Services;
            Builder = new ContainerBuilder();
            Container = default!;
            ServiceProvider = default!;
        }
    }

    /// <summary>
    /// Configures FluentInjections as the DI container for the application.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for service and middleware modules.</param>
    /// <returns>The service collection with FluentInjections configured.</returns>
    public static IServiceCollection AddFluentInjections(this IServiceCollection services, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            Services = services;
            var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            Builder.RegisterModule(new FluentInjectionsModule(services));

            // Build the container
            Container = Builder.Build();

            // Set up Autofac as the service provider
            ServiceProvider = new AutofacServiceProvider(Container);

            // Remove the default service provider
            var serviceProviders = services.Where(s => s.ServiceType == typeof(IServiceProvider)).ToList();

            foreach (var serviceProvider in serviceProviders)
            {
                services.Remove(serviceProvider);
            }

            // Add Autofac service provider
            services.AddSingleton<IServiceProvider>(ServiceProvider);
            services.AddSingleton(ServiceProvider);

            return services;
        }
    }

    /// <summary>
    /// Configures FluentInjections as middleware for the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for middleware modules.</param>
    /// <returns>The application builder with FluentInjections configured.</returns>
    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            var sp = app.ApplicationServices;
            var services = sp.GetRequiredService<IServiceCollection>();
            var registry = sp.GetService<IModuleRegistry>() ?? new ModuleRegistry(services);

            var builder = new ContainerBuilder();
            builder.Populate(services);
            var middlewareConfigurator = new MiddlewareConfigurator(builder);

            foreach (var assembly in targetAssemblies)
            {
                foreach (var type in assembly.GetTypes().Where(type => typeof(IMiddlewareModule).IsAssignableFrom(type) && !type.IsAbstract))
                {
                    var instance = GetInstance<IMiddlewareModule, IMiddlewareConfigurator>(type, sp);
                    if (instance != null)
                    {
                        instance.Configure(middlewareConfigurator);
                        registry.Register<IMiddlewareConfigurator>(typeof(IMiddlewareModule), instance);
                    }
                }
            }

            // Build the final container
            var container = builder.Build();
            var scope = container.Resolve<ILifetimeScope>();
            services.AddSingleton(scope);

            return app;
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

    /// <summary>
    /// Get a named service from the service provider.
    /// </summary>
    /// <typeparam name="TService">The type of service to get.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The service instance.</returns>
    public static TService? GetNamedService<TService>(this IServiceProvider provider, string name) where TService : notnull
    {
        return Container.ResolveNamed<TService>(name);
    }

    /// <summary>
    /// Get a required named service from the service provider.
    /// </summary>
    /// <typeparam name="TService">The type of service to get.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The service instance.</returns>
    public static TService GetRequiredNamedService<TService>(this IServiceProvider provider, string name) where TService : notnull
    {
        return Container.ResolveNamed<TService>(name)
            ?? throw new InvalidOperationException($"Service {name} not found.");
    }
}
