// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;

using FluentInjections;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

/// <summary>
/// Represents a module that provides methods to configure services and middleware within the application.
/// </summary>
internal sealed class FluentInjectionsModule : Autofac.Module
{
    private readonly IServiceCollection _services;
    private readonly Assembly[] _assemblies;

    public FluentInjectionsModule(IServiceCollection services, Assembly[] assemblies)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
    }

    protected override void Load(ContainerBuilder builder)
    {
        var serviceConfigurator = new ServiceConfigurator(builder);
        var middlewareConfigurator = new MiddlewareConfigurator(builder);

        foreach (var assembly in _assemblies)
        {
            RegisterModulesFromAssembly(assembly, serviceConfigurator, middlewareConfigurator);
        }

        serviceConfigurator.Register();
        middlewareConfigurator.Register();
    }

    private void RegisterModulesFromAssembly(Assembly assembly, IServiceConfigurator serviceConfigurator, IMiddlewareConfigurator middlewareConfigurator)
    {
        var moduleTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsPublic)
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
            var configuratorInstance = Activator.CreateInstance(configuratorType);
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
