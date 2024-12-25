using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;

using System.Reflection;
using FluentInjections.Internal.Registries;
using System.Diagnostics;

namespace FluentInjections;

public static class DependencyInjection
{
    /// <summary>
    /// Configures FluentInjections as the DI container for the application.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for service and middleware modules.</param>
    /// <returns>The service collection with FluentInjections configured.</returns>
    public static IServiceCollection AddFluentInjections(this IServiceCollection services, params Assembly[]? assemblies)
    {
        var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

        var builder = new ContainerBuilder();

        builder.RegisterModule(new FluentInjectionsModule(services));

        // Populate the services from the IServiceCollection
        builder.Populate(services);

        var registry = new ModuleRegistry(services);
        var sp = services.BuildServiceProvider();

        foreach (var assembly in targetAssemblies)
        {
            foreach (var type in assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IServiceModule))))
            {
                var instance = GetInstance<IServiceModule>(type, sp) as IModule<IServiceConfigurator>;
                if (instance is not null)
                {
                    registry.Register<IServiceConfigurator>(typeof(IModule<IServiceConfigurator>), instance);
                }
            }
        }

        // Register the module registry
        builder.RegisterInstance(registry).As<IModuleRegistry>();

        // Build the AutoFac container
        var container = builder.Build();

        // Set the service provider factory to use AutoFac
        var serviceProvider = new AutofacServiceProvider(container);

        // Remove the default service provider
        foreach (var service in services.Where(s => s.ServiceType == typeof(IServiceProvider)))
        {
            services.Remove(service);
        }

        // Replace the default service provider with the AutoFac service provider
        services.AddSingleton<IServiceProvider>(serviceProvider);

        return services;
    }

    /// <summary>
    /// Configures FluentInjections as middleware for the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for middleware modules.</param>
    /// <returns>The application builder with FluentInjections configured.</returns>
    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[]? assemblies)
    {
        var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

        var sp = app.ApplicationServices;
        var services = sp.GetRequiredService<IServiceCollection>();
        var registry = sp.GetRequiredService<IModuleRegistry>();

        if (registry is null)
        {
            registry = new ModuleRegistry(services);
        }

        foreach (var assembly in targetAssemblies)
        {
            foreach (var type in assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IMiddlewareModule))))
            {
                var instance = GetInstance<IMiddlewareModule>(type, sp) as IModule<IMiddlewareConfigurator>;
                if (instance is not null)
                {
                    registry.Register<IMiddlewareConfigurator>(typeof(IMiddlewareModule), instance);
                }
            }
        }

        return app;
    }

    private static IModule<IConfigurator>? GetInstance<TModule>(Type type, IServiceProvider? sp = null) where TModule : IModule<IConfigurator>
    {
        if (!type.IsAssignableFrom(typeof(TModule))) return null;

        var instance = sp?.GetService(type) as IModule<IConfigurator>;

        if (instance is not null)
        {
            return instance;
        }

        try
        {
            instance = Activator.CreateInstance(type) as IModule<IConfigurator>;

            if (instance is not null)
            {
                return instance;
            }
        }
        catch { }

        Debug.WriteLine($"Could not create an instance of {type.Name}.");
        return null;
    }

    /// <summary>
    /// Configures AutoFac as the DI container for the application.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="configure">A callback to configure the AutoFac container builder.</param>
    /// <returns>An IServiceProvider backed by AutoFac.</returns>
    internal static IServiceProvider UseAutoFac(this IServiceCollection services, Action<ContainerBuilder> configure)
    {
        // Create a new AutoFac container builder
        var builder = new ContainerBuilder();

        // Populate the services from the IServiceCollection
        builder.Populate(services);

        // Apply additional configuration to the container builder
        configure?.Invoke(builder);

        // Build the AutoFac container
        var container = builder.Build();

        // Return the service provider backed by AutoFac
        return new AutofacServiceProvider(container);
    }
}