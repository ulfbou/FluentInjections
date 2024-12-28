// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Linq;

using FluentInjections;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a registry for service and middleware modules.
/// </summary>
internal class ModuleRegistry : IModuleRegistry
{
    private readonly IServiceCollection _services;
    private readonly ConcurrentDictionary<Type, List<IModule<IConfigurator>>> _modules = new();
    private readonly ConcurrentDictionary<Type, Func<IConfigurableModule<IConfigurator>>> _factories = new();

    public ModuleRegistry(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <inheritdoc />
    public IModuleRegistry Apply<TConfigurator>(TConfigurator configurator) where TConfigurator : IConfigurator
    {
        Guard.NotNull(configurator, nameof(configurator));

        foreach (var module in _modules.Values.SelectMany(m => m))
        {
            if (module.CanHandle<TConfigurator>() && module is IConfigurableModule<TConfigurator> configurableModule)
            {
                configurableModule.Configure(configurator);
            }
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry Initialize()
    {
        foreach (var module in _modules.Values.SelectMany(m => m).OfType<IInitializable>())
        {
            try
            {
                module.Initialize();
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Failed to initialize module of type {module.GetType().Name}.", ex);
            }
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry Register<TModule, TConfigurator>(TModule module) where TModule : IModule<TConfigurator> where TConfigurator : IConfigurator
    {
        Guard.NotNull(module, nameof(module));

        return Register(module.GetType(), module);
    }

    /// <inheritdoc />
    public IModuleRegistry Register<TConfigurator>(Type moduleType, IModule<TConfigurator> module) where TConfigurator : IConfigurator
    {
        Guard.NotNull(moduleType, nameof(moduleType));
        Guard.NotNull(module, nameof(module));

        if (!_modules.ContainsKey(moduleType))
        {
            _modules[moduleType] = new List<IModule<IConfigurator>>();
        }

        if (!(module is IModule<IConfigurator> configuratorModule))
        {
            throw new InvalidOperationException($"Module of type {moduleType.Name} does not implement {typeof(IConfigurableModule<IConfigurator>).Name}.");
        }

        if (_modules[moduleType].Contains(configuratorModule))
        {
            throw new InvalidOperationException($"Module of type {moduleType.Name} is already registered.");
        }

        if (module is IValidatable validatableModule)
        {
            validatableModule.Validate();
        }

        _modules[moduleType].Add(configuratorModule);
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry Register<TModule, TConfigurator>(Func<TModule> factory, Action<TModule>? configure = null)
        where TModule : IModule<TConfigurator>
        where TConfigurator : IConfigurator
    {
        Guard.NotNull(factory, nameof(factory));

        var module = factory();
        configure?.Invoke(module);
        return Register(module.GetType(), module);
    }

    /// <inheritdoc />
    public IModuleRegistry Unregister<TModule, TConfigurator>(TModule module)
        where TModule : IModule<TConfigurator>
        where TConfigurator : IConfigurator
    {
        Guard.NotNull(module, nameof(module));
        return Unregister(module.GetType(), module);
    }

    /// <inheritdoc />
    public IModuleRegistry Unregister<TConfigurator>(Type moduleType, IModule<TConfigurator> module)
        where TConfigurator : IConfigurator
    {
        Guard.NotNull(module, nameof(module));

        if (!(module is IModule<IConfigurator> configuratorModule))
        {
            throw new InvalidOperationException($"Module of type {moduleType.Name} does not implement {typeof(IConfigurableModule<IConfigurator>).Name}.");
        }

        if (!_modules.ContainsKey(moduleType) || !_modules[moduleType].Remove(configuratorModule))
        {
            throw new InvalidOperationException($"Module of type {moduleType.Name} is not registered.");
        }

        return this;
    }

    /// <summary>
    /// Gets all modules registered with the registry.
    /// </summary>
    /// <returns>An enumerable collection of modules.</returns>
    internal IEnumerable<IModule<IConfigurator>> GetAllModules() => _modules.Values.SelectMany(m => m);
}
