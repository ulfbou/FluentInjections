using Autofac.Core;
using Autofac.Core.Registration;

using FluentInjections;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

internal class FluentInjectionsModule : IModule
{
    private readonly IServiceCollection _services;

    public FluentInjectionsModule(IServiceCollection services)
    {
        _services = services;
    }

    public void Configure(IComponentRegistryBuilder componentRegistry)
    {
        // Use reflection to find all types that implement IServiceModule
        // and register them with the container

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var serviceConfigurator = new ServiceConfigurator(_services, componentRegistry);
        var middlewareConfigurator = new MiddlewareConfigurator(_services, componentRegistry);

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // Discover all types that implement IModule<IConfigurator>
                if (type.IsAssignableTo(typeof(IModule)))
                {
                    // Create an instance of the type
                    var instance = Activator.CreateInstance(type);

                    if (instance is IServiceModule serviceModule)
                    {
                        serviceModule.Configure(serviceConfigurator);

                    }
                    else if (instance is IMiddlewareModule middlewareModule)
                    {
                        middlewareModule.Configure(middlewareConfigurator);
                    }
                    else if (instance is IModule module)
                    {
                        module.Configure(componentRegistry);
                    }
                    else if (instance is IModule<IConfigurator> configuratorModule)
                    {
                        var configurator = Activator.CreateInstance(configuratorModule.ConfiguratorType, _services) as IConfigurator;

                        if (configurator is null || configurator is not IConfigurator typedConfigurator)
                        {
                            throw new InvalidOperationException($"Could not create an instance of {configuratorModule.ConfiguratorType.Name}.");
                        }

                        var configuratorType = configuratorModule.ConfiguratorType;

                        if (configuratorType.IsAbstract ||
                            configuratorType.IsInterface ||
                            !configuratorType.IsAssignableFrom(typeof(IConfigurator)))
                        {
                            throw new InvalidOperationException($"Type {configuratorType.Name} does not implement IConfigurator.");
                        }

                        configuratorModule.Configure(typedConfigurator);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Type {type.Name} does not implement IServiceModule, IMiddlewareModule, IModule, or IModule<IConfigurator>.");
                    }
                }
            }
        }

        // For each type that implements IServiceModule
        // Create an instance of the type
        // Cast the instance to IServiceModule
        // Call the Register method on the instance
        // Pass the container to the Register method

    }
}