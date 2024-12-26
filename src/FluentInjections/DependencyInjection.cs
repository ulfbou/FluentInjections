using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;

using System.Reflection;
using FluentInjections.Internal.Registries;
using System.Diagnostics;
using FluentInjections.Internal.Configurators;

namespace FluentInjections;

public static class DependencyInjection
{
    internal static ContainerBuilder Builder { get; private set; }
    internal static AutofacServiceProvider ServiceProvider { get; private set; }
    internal static IContainer Container { get; private set; }

    static DependencyInjection()
    {
        Builder = new ContainerBuilder();
        ServiceProvider = default!;
        Container = default!;
    }

    /// <summary>
    /// Configures FluentInjections as the DI container for the application.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for service and middleware modules.</param>
    /// <returns>The service collection with FluentInjections configured.</returns>
    public static IServiceCollection AddFluentInjections(this IServiceCollection services, params Assembly[]? assemblies)
    {
        var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

        Builder = new ContainerBuilder();
        Builder.Populate(services);
        Builder.RegisterModule(new FluentInjectionsModule(services));

        // Build the container
        Container = Builder.Build();

        // Set up Autofac as the service provider
        ServiceProvider = new AutofacServiceProvider(Container);

        // Remove the default service provider
        if (services.Any(s => s.ServiceType == typeof(IServiceProvider)))
        {
            var serviceProviders = services.Where(s => s.ServiceType == typeof(IServiceProvider));
            foreach (var serviceProvider in serviceProviders)
            {
                services.Remove(serviceProvider);
            }
        }

        services.AddSingleton<IServiceProvider>(ServiceProvider);
        services.AddSingleton<AutofacServiceProvider>();

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

        if (registry == null)
        {
            registry = new ModuleRegistry(services);
        }

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

            if (instance != null)
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

    /// <summary>
    /// Get a named service from the service provider. 
    /// </summary>
    /// <typeparam name="TService">The type of service to get.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The service instance.</returns>
    public static TService? GetNamedService<TService>(this IServiceProvider provider, string name)
    {
        var autofacProvider = provider.GetService<AutofacServiceProvider>()
            ?? provider.GetService<IServiceProvider>() as AutofacServiceProvider
            ?? throw new InvalidOperationException("Initialize FluentInjections before using this method. Make sure to call AddFluentInjections before calling GetNamedService.");

        return autofacProvider.GetKeyedService<TService>(name);
    }
}