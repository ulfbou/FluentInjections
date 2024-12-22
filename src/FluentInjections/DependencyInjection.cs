using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;

using System.Reflection;

namespace FluentInjections;

public static class DependencyInjection
{
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

    public static IServiceCollection AddFluentInjections(this IServiceCollection services, params Assembly[]? assemblies)
    {
        var builder = new ContainerBuilder();



        return services;
    }

    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[] assemblies)
    {

        return app;
    }
    /// <summary>
    /// Configures AutoFac as the DI container for the application and sets it as the default IServiceProvider.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="configure">A callback to configure the AutoFac container builder.</param>
    public static void UseAutoFacAsDefault(this IServiceCollection services, Action<ContainerBuilder> configure)
    {
        // Create a new AutoFac container builder
        var builder = new ContainerBuilder();

        // Populate the services from the IServiceCollection
        builder.Populate(services);

        // Apply additional configuration to the container builder
        configure?.Invoke(builder);

        // Build the AutoFac container
        var container = builder.Build();

        // Set the service provider factory to use AutoFac
        var serviceProviderFactory = new AutofacServiceProviderFactory();
        //var serviceProvider = serviceProviderFactory.CreateServiceProvider(container);

        // Replace the default service provider with the AutoFac service provider
        //services.AddSingleton<IServiceProvider>(serviceProvider);
    }
}