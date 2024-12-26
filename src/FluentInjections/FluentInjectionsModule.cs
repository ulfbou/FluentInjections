using Autofac;
using Autofac.Core;

using FluentInjections;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections.Tests;

internal class FluentInjectionsModule : Module
{
    private readonly IServiceCollection _services;

    public FluentInjectionsModule(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    protected override void Load(ContainerBuilder builder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var serviceConfigurator = new ServiceConfigurator(builder);
        var middlewareConfigurator = new MiddlewareConfigurator(builder);

        foreach (var assembly in assemblies)
        {
            RegisterModulesFromAssembly(assembly, serviceConfigurator, middlewareConfigurator);
        }

        serviceConfigurator.Register();
        middlewareConfigurator.Register();
    }

    private void RegisterModulesFromAssembly(Assembly assembly, IServiceConfigurator serviceConfigurator, IMiddlewareConfigurator middlewareConfigurator)
    {
        var moduleTypes = assembly.GetTypes()
                                  .Where(t => !t.IsAbstract && !t.IsInterface)
                                  .SelectMany(t => t.GetInterfaces()
                                                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>))
                                                    .Select(i => new { ModuleType = t, Interface = i }))
                                  .ToList();

        foreach (var moduleType in moduleTypes)
        {
            RegisterModule(moduleType.ModuleType, moduleType.Interface, serviceConfigurator, middlewareConfigurator);
        }
    }

    private void RegisterModule(Type moduleType, Type interfaceType, IServiceConfigurator serviceConfigurator, IMiddlewareConfigurator middlewareConfigurator)
    {
        var configuratorType = interfaceType.GetGenericArguments().First();
        var instance = Activator.CreateInstance(moduleType);
        var configureMethod = moduleType.GetMethod("Configure");

        if (configureMethod is null)
        {
            throw new InvalidOperationException($"No suitable Configure method found for module type {moduleType.Name}");
        }

        if (configuratorType == typeof(IServiceConfigurator))
        {
            configureMethod.Invoke(instance, new object[] { serviceConfigurator });
        }
        else if (configuratorType == typeof(IMiddlewareConfigurator))
        {
            configureMethod.Invoke(instance, new object[] { middlewareConfigurator });
        }
        else if (!configuratorType.IsAbstract)
        {
            var configuratorInstance = Activator.CreateInstance(configuratorType, _services, builder.ComponentRegistry) as IConfigurator;
            if (configuratorInstance is null)
            {
                throw new InvalidOperationException($"Failed to create an instance of {configuratorType.Name} for module type {moduleType.Name}");
            }
            configureMethod.Invoke(instance, new object[] { configuratorInstance });
        }
        else
        {
            throw new InvalidOperationException($"No suitable configurator found for module type {moduleType.Name}");
        }
    }
}