using FluentInjections.Internal.Modules;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a registry for service and middleware modules.
/// </summary>
internal class ModuleRegistry<TBuilder> : IModuleRegistry<TBuilder> where TBuilder : class
{
    protected readonly List<IServiceModule> _serviceModules = new();
    protected readonly List<IMiddlewareModule<TBuilder>> _middlewareModules = new();

    // TODO: Add logger and configuration properties

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule(IServiceModule module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        _serviceModules.Add(module);
        return this;
    }


    /// <inheritdoc />
    public virtual IModuleRegistry<TBuilder> UnregisterModule(IServiceModule module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        _serviceModules.Remove(module);
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule(IMiddlewareModule<TBuilder> module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        _middlewareModules.Add(module);
        return this;
    }

    /// <inheritdoc/>
    public IModuleRegistry<TBuilder> UnregisterModule(IMiddlewareModule<TBuilder> module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        _middlewareModules.Remove(module);
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new()
    {
        ArgumentGuard.NotNull(factory, nameof(factory));

        var instance = factory();
        configure?.Invoke(instance);

        if (instance is IServiceModule serviceModule)
        {
            RegisterModule(serviceModule);
        }
        else if (instance is IMiddlewareModule<TBuilder> middlewareModule)
        {
            RegisterModule(middlewareModule);
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule<T>(Func<bool> condition) where T : IServiceModule, new()
    {
        ArgumentGuard.NotNull(condition, nameof(condition));

        if (condition())
        {
            RegisterModule(new T());
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> ApplyServiceModules(IServiceConfigurator serviceConfigurator)
    {
        ArgumentGuard.NotNull(serviceConfigurator, nameof(serviceConfigurator));

        foreach (var module in _serviceModules)
        {
            module.ConfigureServices(serviceConfigurator);
        }

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator)
    {
        ArgumentGuard.NotNull(middlewareConfigurator, nameof(middlewareConfigurator));

        foreach (var module in _middlewareModules)
        {
            module.ConfigureMiddleware(middlewareConfigurator);
        }
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> InitializeModules()
    {
        foreach (var module in _serviceModules)
        {
            if (module is IInitializable initializable)
            {
                initializable.Initialize();
            }
        }

        foreach (var module in _middlewareModules)
        {
            if (module is IInitializable initializable)
            {
                initializable.Initialize();
            }
        }

        return this;
    }

    /// <inheritdoc />
    public virtual bool CanHandle<TModule>() where TModule : class, IServiceModule => _serviceModules.OfType<TModule>().Any();

    /// <inheritdoc />
    public virtual bool CanHandle(Type type) => _serviceModules.Any(m => m.GetType() == type);
}
