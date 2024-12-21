using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Modules;
using FluentInjections.Internal.Registries;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

public static class DependencyInjection
{
    public static IServiceCollection AddFluentInjections<TBuilder>(this IServiceCollection services, params Assembly[]? assemblies)
        where TBuilder : class
    {
        ArgumentGuard.NotNull(services, nameof(services));

        var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);

        var moduleRegistry = new ModuleRegistry<TBuilder>();

        // Register modules
        services.RegisterModules<TBuilder, IServiceModule>(targetAssemblies.AsParallel(), module => moduleRegistry.RegisterModule(module));
        services.RegisterModules<TBuilder, IMiddlewareModule<TBuilder>>(targetAssemblies.AsParallel(), module => moduleRegistry.RegisterModule(module));

        var serviceConfigurator = new ServiceConfigurator(services);
        moduleRegistry.ApplyServiceModules(serviceConfigurator);

        // Build the intermediate container
        var intermediateContainer = containerBuilder.Build();
        var serviceProvider = new AutofacServiceProvider(intermediateContainer);

        // Resolve TBuilder from the intermediate container
        using (var scope = intermediateContainer.BeginLifetimeScope())
        {
            var builder = scope.Resolve<TBuilder>() ?? throw new InvalidOperationException($"No service of type {typeof(TBuilder).Name} was found.");
            var middlewareConfigurator = new MiddlewareConfigurator<TBuilder>(services, builder);
            moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);
        }

        // Build the final container
        containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        var finalContainer = containerBuilder.Build();

        // Register ILifetimeScope and other services
        services.AddSingleton(finalContainer.Resolve<ILifetimeScope>());
        services.AddSingleton<IModuleRegistry<TBuilder>>(moduleRegistry);
        services.AddSingleton<IServiceProvider>(new AutofacServiceProvider(finalContainer));

        return services;
    }

    internal static void RegisterModules<TBuilder, TModule>(this IServiceCollection services, ParallelQuery<Assembly> assemblies, Action<TModule> registerAction)
        where TBuilder : class
        where TModule : class
    {
        var modules = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(TModule).IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic && !type.ContainsGenericParameters)
            .Select(type => services.ResolveMiddleware(type))
            .OfType<TModule>();

        foreach (var module in modules)
        {
            registerAction(module);
        }
    }

    private static object? ResolveMiddleware(this IServiceCollection services, Type type)
    {
        var constructor = type.GetConstructors().FirstOrDefault();
        var parameters = constructor?.GetParameters();
        var resolvedParameters = parameters?.Select(parameter => services.ResolveParameter(parameter)).ToArray();
        return constructor?.Invoke(resolvedParameters);
    }

    private static object? ResolveParameter(this IServiceCollection services, ParameterInfo parameter)
    {
        var parameterType = parameter.ParameterType;
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService(parameterType);
        return service ?? Activator.CreateInstance(parameterType);
    }

    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[] assemblies)
    {
        IServiceProvider sp = app.ApplicationServices;
        var moduleRegistry = app.ApplicationServices.GetRequiredService<IModuleRegistry<IApplicationBuilder>>();
        var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        var services = app.ApplicationServices.GetRequiredService<IServiceCollection>();

        services.RegisterModules<IApplicationBuilder, IMiddlewareModule<IApplicationBuilder>>(targetAssemblies.AsParallel(), module => moduleRegistry.RegisterModule(module));

        var middlewareConfigurator = new MiddlewareConfigurator<IApplicationBuilder>(services, app);
        moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

        return app;
    }

    public static IApplicationBuilder UseFluentInjections<TRegistry>(this IApplicationBuilder app, params Assembly[] assemblies)
        where TRegistry : IModuleRegistry<IApplicationBuilder>
    {
        var moduleRegistry = app.ApplicationServices.GetRequiredService<TRegistry>();
        var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        var services = app.ApplicationServices.GetRequiredService<IServiceCollection>();
        services.RegisterModules<IApplicationBuilder, IMiddlewareModule<IApplicationBuilder>>(targetAssemblies.AsParallel(), module => moduleRegistry.RegisterModule(module));

        var middlewareConfigurator = new MiddlewareConfigurator<IApplicationBuilder>(services, app);
        moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

        return app;
    }
}