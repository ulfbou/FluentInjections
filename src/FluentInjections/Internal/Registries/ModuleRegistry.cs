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

    public IModuleRegistry Apply<TConfigurator>(TConfigurator configurator) where TConfigurator : IConfigurator
    {
        ArgumentGuard.NotNull(configurator, nameof(configurator));

        foreach (var module in _modules.Values.SelectMany(m => m))
        {
            if (module.CanHandle<TConfigurator>() && module is IConfigurableModule<TConfigurator> configurableModule)
            {
                configurableModule.Configure(configurator);
            }
        }

        return this;
    }

    public IModuleRegistry Initialize()
    {
        foreach (var module in _modules.Values.SelectMany(m => m).OfType<IInitializable>())
        {
            module.Initialize();
        }
        return this;
    }

    public IModuleRegistry Register<TModule, TConfigurator>(TModule module) where TModule : IModule<TConfigurator> where TConfigurator : IConfigurator =>
        Register(module.GetType(), module);

    public IModuleRegistry Register<TConfigurator>(Type moduleType, IModule<TConfigurator> module) where TConfigurator : IConfigurator
    {
        ArgumentGuard.NotNull(moduleType, nameof(moduleType));
        ArgumentGuard.NotNull(module, nameof(module));

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

        _modules[moduleType].Add(configuratorModule);
        return this;
    }

    public IModuleRegistry Register<TModule, TConfigurator>(Func<TModule> factory, Action<TModule>? configure = null)
        where TModule : IModule<TConfigurator>
        where TConfigurator : IConfigurator
    {
        ArgumentGuard.NotNull(factory, nameof(factory));

        var module = factory();
        configure?.Invoke(module);
        return Register(module.GetType(), module);
    }

    public IModuleRegistry Unregister<TModule, TConfigurator>(TModule module)
        where TModule : IModule<TConfigurator>
        where TConfigurator : IConfigurator
        => Unregister(module.GetType(), module);

    public IModuleRegistry Unregister<TConfigurator>(Type moduleType, IModule<TConfigurator> module)
        where TConfigurator : IConfigurator
    {
        ArgumentGuard.NotNull(module, nameof(module));

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
