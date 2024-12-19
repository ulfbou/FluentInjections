using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

public static class DependencyInjection
{
    public static IServiceCollection AddFluentInjections<TBuilder, TRegistry>(this IServiceCollection services, params Assembly[] assemblies)
        where TBuilder : class
        where TRegistry : IModuleRegistry<TBuilder>, new()
    {
        // Create a scope to efficiently maintain a reflection cache for the target assemblies.
        var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

        // Create an Autofac container builder
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);

        // Build the intermediate service provider
        var serviceProvider = new AutofacServiceProvider(containerBuilder.Build());
        var moduleRegistry = new ModuleRegistry<TBuilder>();
        RegisterModules<TBuilder, IServiceModule>(targetAssemblies, moduleRegistry.RegisterModule);
        RegisterModules<TBuilder, IMiddlewareModule<TBuilder>>(targetAssemblies, moduleRegistry.RegisterModule);

        // Apply service modules
        var serviceConfigurator = new ServiceConfigurator(services);
        moduleRegistry.ApplyServiceModules(serviceConfigurator);

        // Build the final service provider
        containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        var finalContainer = containerBuilder.Build();

        services.AddSingleton<IModuleRegistry<TBuilder>>(moduleRegistry);
        services.AddSingleton<IServiceProvider>(new AutofacServiceProvider(finalContainer));

        return services;
    }

    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[] assemblies)
    {
        var moduleRegistry = app.ApplicationServices.GetRequiredService<IModuleRegistry<IApplicationBuilder>>();
        var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        RegisterModules<IApplicationBuilder, IMiddlewareModule<IApplicationBuilder>>(targetAssemblies, moduleRegistry.RegisterModule);

        var services = app.ApplicationServices.GetRequiredService<IServiceCollection>();
        var middlewareConfigurator = new MiddlewareConfigurator<IApplicationBuilder>(services, app);
        moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

        return app;
    }

    public static IApplicationBuilder UseFluentInjections<TRegistry>(this IApplicationBuilder app, params Assembly[] assemblies)
        where TRegistry : IModuleRegistry<IApplicationBuilder>
    {
        var moduleRegistry = app.ApplicationServices.GetRequiredService<TRegistry>();
        var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        RegisterModules<IApplicationBuilder, IMiddlewareModule<IApplicationBuilder>>(targetAssemblies, moduleRegistry.RegisterModule);

        var services = app.ApplicationServices.GetRequiredService<IServiceCollection>();
        var middlewareConfigurator = new MiddlewareConfigurator<IApplicationBuilder>(services, app);
        moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

        return app;
    }

    private static void RegisterModules<TBuilder, TModule>(Assembly[] assemblies, Func<TModule, IModuleRegistry<TBuilder>> registerAction)
        where TBuilder : class
        where TModule : class
    {
        var modules = assemblies
            .AsParallel()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(TModule).IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic && !type.ContainsGenericParameters)
            .Select(type => Activator.CreateInstance(type))
            .OfType<TModule>();

        foreach (var module in modules)
        {
            registerAction(module);
        }
    }
}
