using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

/// <summary>
/// Represents a registry of modules.
/// </summary>
public class ModuleRegistry<TBuilder> : IModuleRegistry<TBuilder>
{
    protected readonly List<IServiceModule> _serviceModules = new();
    protected readonly List<IMiddlewareModule<TBuilder>> _middlewareModules = new();

    /// <summary>
    /// Registers a module with a condition.
    /// </summary>
    /// <typeparam name="T">The module type.</typeparam>
    /// <param name="condition">The condition to determine if the module should be registered.</param>
    public IModuleRegistry<TBuilder> RegisterModule<T>(Func<bool> condition) where T : IServiceModule, new()
    {
        if (condition())
        {
            _serviceModules.Add(new T());
        }

        return this;
    }

    /// <summary>
    /// Registers a service module.
    /// </summary>
    /// <param name="module">The service module.</param>
    public IModuleRegistry<TBuilder> RegisterModule(IServiceModule module)
    {
        ArgumentNullException.ThrowIfNull(module, nameof(module));

        _serviceModules.Add(module);
        return this;
    }

    /// <summary>
    /// Registers a middleware module.
    /// </summary>
    /// <param name="module">The middleware module.</param>
    public IModuleRegistry<TBuilder> RegisterModule(IMiddlewareModule<TBuilder> module)
    {
        ArgumentNullException.ThrowIfNull(module, nameof(module));

        _middlewareModules.Add(module);
        return this;
    }

    /// <summary>
    /// Registers a module with a factory and optional configuration.
    /// </summary>
    /// <typeparam name="T">The module type.</typeparam>
    /// <param name="factory">The factory to create the module.</param>
    /// <param name="configure">The optional configuration action.</param>
    public IModuleRegistry<TBuilder> RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        _serviceModules.Add(new LazyServiceModule<T>(factory, configure));
        return this;
    }

    /// <summary>
    /// Applies the service modules to the service configurator.
    /// </summary>
    /// <param name="serviceConfigurator">The service configurator.</param>
    public IModuleRegistry<TBuilder> ApplyServiceModules(IServiceConfigurator serviceConfigurator)
    {
        ArgumentNullException.ThrowIfNull(serviceConfigurator, nameof(serviceConfigurator));

        foreach (var module in _serviceModules)
        {
            module.ConfigureServices(serviceConfigurator);
            (module as IValidatable)?.Validate();
        }

        return this;
    }

    /// <summary>
    /// Applies the middleware modules to the middleware configurator.
    /// </summary>
    /// <param name="middlewareConfigurator">The middleware configurator.</param>
    public IModuleRegistry<TBuilder> ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator)
    {
        ArgumentNullException.ThrowIfNull(middlewareConfigurator, nameof(middlewareConfigurator));

        foreach (var module in _middlewareModules)
        {
            module.ConfigureMiddleware(middlewareConfigurator);
            (module as IValidatable)?.Validate();
        }

        return this;
    }

    /// <summary>
    /// Initializes the modules.
    /// </summary>
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

    /// <summary>
    /// Determines if the registry can handle a module of the specified type.
    /// </summary>
    /// <typeparam name="TModule">The module type.</typeparam>
    public virtual bool CanHandle<TModule>() where TModule : class, IServiceModule => CanHandle(typeof(TModule));

    /// <summary>
    /// Determines if the registry can handle a module of the specified type.
    /// </summary>
    /// <param name="moduleType">The module type.</param>
    public virtual bool CanHandle(Type moduleType)
    {
        if (GetType() != typeof(ModuleRegistry<TBuilder>))
        {
            // TODO: This should be a more specific exception
            throw new InvalidRegistrationException($"Since '{this.GetType().Name}' extends ModuleRegistry it must override the CanHandle method to notify the CompositeModuleRegistry if it can handle a given module.");
        }

        throw new InvalidRegistrationException("The ModuleRegistry has been registered to handle modules. However, it should only be used as a fallback registry, which FluentInjections already handles.");
    }
}
