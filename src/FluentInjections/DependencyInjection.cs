﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

public static class DependencyInjection
{
    public static IServiceCollection AddFluentInjections<TBuilder>(this IServiceCollection services, params Assembly[] assemblies)
    {
        var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        var moduleRegistry = new ModuleRegistry<TBuilder>();

        RegisterModules<IServiceModule>(targetAssemblies, moduleRegistry.RegisterModule);
        RegisterModules<IMiddlewareModule<TBuilder>>(targetAssemblies, moduleRegistry.RegisterModule);

        var serviceConfigurator = new ServiceConfigurator(services);
        moduleRegistry.ApplyServiceModules(serviceConfigurator);

        services.AddSingleton(moduleRegistry);

        return services;
    }

    public static IServiceCollection AddFluentInjections<TBuilder, TRegistry>(this IServiceCollection services, params Assembly[] assemblies)
        where TRegistry : ModuleRegistry<TBuilder>, new()
    {
        var serviceProvider = services.BuildServiceProvider();
        var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        var moduleRegistry = serviceProvider.GetRequiredService<TRegistry>();

        RegisterModules<IServiceModule>(targetAssemblies, moduleRegistry.RegisterModule);
        RegisterModules<IMiddlewareModule<TBuilder>>(targetAssemblies, moduleRegistry.RegisterModule);

        var serviceConfigurator = new ServiceConfigurator(services);
        moduleRegistry.ApplyServiceModules(serviceConfigurator);

        services.AddSingleton<ModuleRegistry<TBuilder>>(moduleRegistry);

        return services;
    }

    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app)
    {
        var moduleRegistry = app.ApplicationServices.GetRequiredService<ModuleRegistry<IApplicationBuilder>>();
        var middlewareConfigurator = new MiddlewareConfigurator<IApplicationBuilder>(app);

        moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

        return app;
    }

    private static void RegisterModules<TModule>(Assembly[] assemblies, Action<TModule> registerAction)
        where TModule : class
    {
        var modules = assemblies
            .AsParallel()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(TModule).IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic)
            .Select(Activator.CreateInstance)
            .Cast<TModule>();

        foreach (var module in modules)
        {
            registerAction(module);
        }
    }
}