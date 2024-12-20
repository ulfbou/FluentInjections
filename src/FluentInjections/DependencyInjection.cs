using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Modules;
using FluentInjections.Internal.Registries;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

public static class DependencyInjection
{
    public static IServiceCollection AddFluentInjections<TBuilder, TRegistry>(this IServiceCollection services, params Assembly[]? assemblies)
        where TBuilder : class
        where TRegistry : IModuleRegistry<TBuilder>, new()
    {
        ArgumentGuard.NotNull(services, nameof(services));

        // Create a scope to efficiently maintain a reflection cache for the target assemblies.
        var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

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

        // Resolve TBuilder from the service provider
        var builder = serviceProvider.GetService<TBuilder>() ?? throw new InvalidOperationException($"No service of type {typeof(TBuilder).Name} was found.");
        var middlewareConfigurator = new MiddlewareConfigurator<TBuilder>(services, builder);
        moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

        // Build the final service provider
        containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);

        // Register ILifetimeScope
        var finalContainer = containerBuilder.Build();
        services.AddSingleton(finalContainer.Resolve<ILifetimeScope>());

        services.AddSingleton<IModuleRegistry<TBuilder>>(moduleRegistry);
        services.AddSingleton<IServiceProvider>(new AutofacServiceProvider(finalContainer));

        return services;
    }

    private static void RegisterModules<TBuilder, TModule>(Assembly[] assemblies, ContainerBuilder containerBuilder)
        where TBuilder : class
        where TModule : class
    {
        var modules = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(TModule).IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic && !type.ContainsGenericParameters)
            .Select(type => Activator.CreateInstance(type))
            .OfType<TModule>();

        foreach (var module in modules)
        {
            containerBuilder.RegisterInstance(module).As<TModule>();
        }
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
            //.AsParallel()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(TModule).IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic && !type.ContainsGenericParameters)
            .Select(type => ResolveMiddleware(type))
            .OfType<TModule>();

        foreach (var module in modules)
        {
            registerAction(module);
        }
    }

    private static object? ResolveMiddleware(Type type)
    {
        var constructor = type.GetConstructors().FirstOrDefault();
        var parameters = constructor?.GetParameters();
        var resolvedParameters = parameters?.Select(parameter => ResolveParameter(parameter)).ToArray();
        return constructor?.Invoke(resolvedParameters);
    }

    private static object? ResolveParameter(ParameterInfo parameter)
    {
        var parameterType = parameter.ParameterType;
        var service = Activator.CreateInstance(parameterType);
        return service;
    }

    public static T? GetNamedService<T>(this IServiceProvider provider, string name) where T : class
    {
        var serviceCollection = provider.GetRequiredService<IServiceCollection>();
        var serviceDescriptor = serviceCollection.FirstOrDefault(
                descriptor => descriptor.ServiceType == typeof(T) &&
                descriptor is NamedServiceDescriptor named &&
                named.Name == name);

        return serviceDescriptor?.ImplementationInstance as T;
    }

    public static T? GetNamedService<T>(this IServiceProvider provider, string name, T defaultValue) where T : class
    {
        var service = provider.GetNamedService<T>(name);
        return service ?? defaultValue;
    }

    public static T GetRequiredNamedService<T>(this IServiceProvider provider, string name) where T : class
    {
        var service = provider.GetNamedService<T>(name);
        if (service is null)
        {
            throw new InvalidOperationException($"No service of type {typeof(T).Name} with the name '{name}' was found.");
        }

        return service;
    }

    public static T GetRequiredNamedService<T>(this IServiceProvider provider, string name, T defaultValue) where T : class
    {
        var service = provider.GetNamedService<T>(name);
        return service ?? defaultValue;
    }
}
