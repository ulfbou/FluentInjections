using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

public static class DependencyInjection
{
    public static IServiceCollection AddFluentInjections(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Use provided assemblies or load all assemblies in the current domain
        var targetAssemblies = assemblies.Length > 0
            ? assemblies
            : AppDomain.CurrentDomain.GetAssemblies();

        // Create a registry to hold the modules
        var moduleRegistry = new ModuleRegistry();

        // Discover and register IServiceModules
        var serviceModules = DiscoverModules<IServiceModule>(targetAssemblies);

        foreach (var module in serviceModules)
        {
            moduleRegistry.RegisterModule(module);
        }

        // Discover and register IMiddlewareModules
        var middlewareModules = DiscoverModules<IMiddlewareModule>(targetAssemblies);
        foreach (var module in middlewareModules)
        {
            moduleRegistry.RegisterModule(module);
        }

        // Apply service configurations
        var serviceConfigurator = new ServiceConfigurator(services);
        moduleRegistry.ApplyServiceModules(serviceConfigurator);

        // Middleware configuration is deferred until app building
        services.AddSingleton(moduleRegistry);

        return services;
    }

    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app)
    {
        var moduleRegistry = app.ApplicationServices.GetRequiredService<ModuleRegistry>();
        var middlewareConfigurator = new MiddlewareConfigurator(app);

        moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

        return app;
    }


    private static IEnumerable<TModule> DiscoverModules<TModule>(Assembly[] assemblies)
        where TModule : class
    {
        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(TModule).IsAssignableFrom(type) &&
                           !type.IsAbstract &&
                           type.IsPublic)
            .Select(Activator.CreateInstance)
            .Cast<TModule>();
    }
}
