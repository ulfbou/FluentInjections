using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Modules;
using FluentInjections.Internal.Registries;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFluentInjections<TBuilder, TRegistry>(this IServiceCollection services, params Assembly[]? assemblies)
            where TBuilder : class
            where TRegistry : IModuleRegistry<TBuilder>, new()
        {
            ArgumentGuard.NotNull(services, nameof(services));

            var targetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);

            var moduleRegistry = new ModuleRegistry<TBuilder>();

            RegisterModules<TBuilder, IServiceModule>(targetAssemblies.AsParallel(), moduleRegistry.RegisterModule);
            RegisterModules<TBuilder, IMiddlewareModule<TBuilder>>(targetAssemblies.AsParallel(), moduleRegistry.RegisterModule);

            var serviceConfigurator = new ServiceConfigurator(services);
            moduleRegistry.ApplyServiceModules(serviceConfigurator);

            var intermediateContainer = containerBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(intermediateContainer);

            var builder = serviceProvider.GetService<TBuilder>() ?? throw new InvalidOperationException($"No service of type {typeof(TBuilder).Name} was found.");
            var middlewareConfigurator = new MiddlewareConfigurator<TBuilder>(services, builder);
            moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

            containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);

            var finalContainer = containerBuilder.Build();
            services.AddSingleton(finalContainer.Resolve<ILifetimeScope>());
            services.AddSingleton<IModuleRegistry<TBuilder>>(moduleRegistry);
            services.AddSingleton<IServiceProvider>(new AutofacServiceProvider(finalContainer));

            return services;
        }

        private static void RegisterModules<TBuilder, TModule>(ParallelQuery<Assembly> assemblies, Action<TModule> registerAction)
            where TBuilder : class
            where TModule : class
        {
            var modules = assemblies
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
            var serviceProvider = new ServiceCollection().BuildServiceProvider(); // Replace with actual service provider if available
            var service = serviceProvider.GetService(parameterType);
            return service ?? Activator.CreateInstance(parameterType);
        }

        public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder app, params Assembly[] assemblies)
        {
            var moduleRegistry = app.ApplicationServices.GetRequiredService<IModuleRegistry<IApplicationBuilder>>();
            var targetAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            RegisterModules<IApplicationBuilder, IMiddlewareModule<IApplicationBuilder>>(targetAssemblies.AsParallel(), moduleRegistry.RegisterModule);

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
            RegisterModules<IApplicationBuilder, IMiddlewareModule<IApplicationBuilder>>(targetAssemblies.AsParallel(), moduleRegistry.RegisterModule);

            var services = app.ApplicationServices.GetRequiredService<IServiceCollection>();
            var middlewareConfigurator = new MiddlewareConfigurator<IApplicationBuilder>(services, app);
            moduleRegistry.ApplyMiddlewareModules(middlewareConfigurator);

            return app;
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
}