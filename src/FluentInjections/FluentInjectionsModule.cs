// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

/// <summary>
/// Represents a module that provides methods to configure services and middleware within the application.
/// </summary>
internal abstract class FluentInjectionsModule
{
    protected readonly Assembly[] _assemblies;

    public FluentInjectionsModule(Assembly[] assemblies)
    {
        _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
    }

    protected void RegisterModules<TConfigurator, TModule>(Assembly assembly, TConfigurator configurator)
        where TConfigurator : IConfigurator
        where TModule : IConfigurableModule<TConfigurator>
    {
        var types = assembly.GetTypes()
                            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsPublic);
        var modules = types
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>) &&
                i.GetGenericArguments()[0] == typeof(TConfigurator)))
            .Select(t => new ModuleType(t, t.GetInterfaces().First(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>) &&
                i.GetGenericArguments()[0] == typeof(TConfigurator))));

        foreach (var moduleType in modules)
        {
            RegisterModule(moduleType.Module, moduleType.Interface, configurator);
        }
    }

    protected void RegisterModule<TConfigurator>(Type moduleType, Type interfaceType, TConfigurator configurator) where TConfigurator : IConfigurator
    {
        var configuratorType = interfaceType.GetGenericArguments().First();
        var instance = Activator.CreateInstance(moduleType);
        var configureMethod = moduleType.GetMethod("Configure");

        if (configureMethod is null)
        {
            throw new InvalidOperationException($"No suitable Configure method found for module type {moduleType.Name}");
        }

        if (configuratorType == typeof(TConfigurator))
        {
            configureMethod.Invoke(instance, new object[] { configurator });
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
