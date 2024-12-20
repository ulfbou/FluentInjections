using FluentInjections.Internal.Constants;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;

namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a composite registry of modules.
/// </summary>
public class CompositeModuleRegistry<TBuilder> : IModuleRegistry<TBuilder> where TBuilder : class
{
    private readonly ConcurrentBag<IModuleRegistry<TBuilder>> _registries = new();
    private readonly IServiceProvider _serviceProvider;

    public CompositeModuleRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> AddRegistry(IModuleRegistry<TBuilder> registry)
    {
        ArgumentGuard.NotNull(registry, nameof(registry));

        _registries.Add(registry);
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule(IServiceModule module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        if (!TryRegisterModule(module.GetType(), registry => registry.RegisterModule(module)))
        {
            var moduleRegistry = _serviceProvider.GetRequiredService<ModuleRegistry<TBuilder>>();
            moduleRegistry.RegisterModule(module);
        }

        return this;
    }

    /// <inheritdoc/>
    public IModuleRegistry<TBuilder> UnregisterModule(IServiceModule module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        if (!TryRegisterModule(module.GetType(), registry => registry.UnregisterModule(module)))
        {
            throw new InvalidRegistrationException($"No registry was found that can handle the module type {module.GetType().Name}.");
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule(IMiddlewareModule<TBuilder> module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        if (!TryRegisterModule(module.GetType(), registry => registry.RegisterModule(module)))
        {
            var moduleRegistry = _serviceProvider.GetRequiredService<ModuleRegistry<TBuilder>>();
            moduleRegistry.RegisterModule(module);
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> UnregisterModule(IMiddlewareModule<TBuilder> module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        if (!TryRegisterModule(module.GetType(), registry => registry.UnregisterModule(module)))
        {
            throw new InvalidRegistrationException($"No registry was found that can handle the module type {module.GetType().Name}.");
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new()
    {
        ArgumentGuard.NotNull(factory, nameof(factory));

        if (!TryRegisterModule(typeof(T), registry => registry.RegisterModule(factory, configure)))
        {
            var moduleRegistry = _serviceProvider.GetRequiredService<ModuleRegistry<TBuilder>>();
            moduleRegistry.RegisterModule(factory, configure);
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule<T>(Func<bool> condition) where T : IServiceModule, new()
    {
        ArgumentGuard.NotNull(condition, nameof(condition));

        if (!TryRegisterModule(typeof(T), registry => registry.RegisterModule<T>(condition)))
        {
            throw new InvalidRegistrationException($"No registry was found that can handle the module type {typeof(T).Name}.");
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> ApplyServiceModules(IServiceConfigurator serviceConfigurator)
    {
        ArgumentGuard.NotNull(serviceConfigurator, nameof(serviceConfigurator));

        Parallel.ForEach(_registries, registry => registry.ApplyServiceModules(serviceConfigurator));
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator)
    {
        ArgumentGuard.NotNull(middlewareConfigurator, nameof(middlewareConfigurator));

        Parallel.ForEach(_registries, registry => registry.ApplyMiddlewareModules(middlewareConfigurator));
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> InitializeModules()
    {
        ArgumentGuard.NotNull(_registries, nameof(_registries));

        Parallel.ForEach(_registries, registry => registry.InitializeModules());
        return this;
    }

    /// <inheritdoc />
    public bool CanHandle(Type moduleType)
        => throw new InvalidRegistrationException(ErrorMessages.Composite.CallCanHandle);

    /// <inheritdoc />
    public bool CanHandle<TModule>() where TModule : class, IServiceModule
        => throw new InvalidRegistrationException(ErrorMessages.Composite.CallCanHandle);

    private bool TryRegisterModule(Type moduleType, Action<IModuleRegistry<TBuilder>> registerAction)
    {
        foreach (var registry in _registries)
        {
            if (registry.CanHandle(moduleType))
            {
                registerAction(registry);
                return true;
            }
        }
        return false;
    }
}
