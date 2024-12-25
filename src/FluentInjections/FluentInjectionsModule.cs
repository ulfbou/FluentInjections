using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;

using FluentInjections;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

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
        var serviceConfigurator = new ServiceConfigurator(new ContainerBuilder());
        var middlewareConfigurator = new MiddlewareConfigurator(_services, componentRegistry);

        foreach (var assembly in assemblies)
        {
            // Discover all types that implement IConfigurableModule<>
            foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var interfaces = type.GetInterfaces().Where(t => t.IsGenericType);

                if (!interfaces.Any(i => i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>)))
                {
                    continue;
                }

                try
                {
                    var genericType = interfaces.First(i => i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>));
                    var configuratorType = genericType.GetGenericArguments().First();
                    var instance = Activator.CreateInstance(type);
                    var method = type.GetMethod("Configure");

                    if (method is null)
                    {
                        throw new InvalidOperationException($"No suitable method found for module type {type.Name}");
                    }

                    if (configuratorType == typeof(IServiceConfigurator))
                    {
                        method.Invoke(instance, new object[] { serviceConfigurator });
                    }
                    else if (configuratorType == typeof(IMiddlewareConfigurator))
                    {
                        method.Invoke(instance, new object[] { middlewareConfigurator });
                    }
                    else if (configuratorType.IsAbstract)
                    {
                        throw new InvalidOperationException($"No suitable configurator found for module type {type.Name}");
                    }
                    else
                    {
                        // Treat any implementation of IConfigurator as special case for now and attempt to create a configurator instance for it.
                        var configuratorInstance = Activator.CreateInstance(configuratorType, _services, componentRegistry) as IConfigurator;

                        if (configuratorInstance is null)
                        {
                            throw new InvalidOperationException($"No suitable configurator found for module type {type.Name}");
                        }

                        method.Invoke(instance, new object[] { configuratorInstance });
                    }
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
        }
    }
}
