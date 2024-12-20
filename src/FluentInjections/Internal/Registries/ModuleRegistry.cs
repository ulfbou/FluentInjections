﻿using FluentInjections.Internal.Modules;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;

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

        if (_serviceModules.Contains(module))
        {
            throw new InvalidOperationException($"The module {module.GetType().Name} is already registered.");
        }

        _serviceModules.Add(module);
        Debug.WriteLine($"Registered service module {module.GetType().Name}.");

        return this;
    }


    /// <inheritdoc />
    public virtual IModuleRegistry<TBuilder> UnregisterModule(IServiceModule module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        if (!_serviceModules.Contains(module))
        {
            throw new InvalidOperationException($"The module {module.GetType().Name} is not registered.");
        }

        _serviceModules.Remove(module);
        Debug.WriteLine($"Unregistered service module {module.GetType().Name}.");

        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> RegisterModule(IMiddlewareModule<TBuilder> module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        _middlewareModules.Add(module);
        Debug.WriteLine($"Registered middleware module {module.GetType().Name}.");

        return this;
    }

    /// <inheritdoc/>
    public IModuleRegistry<TBuilder> UnregisterModule(IMiddlewareModule<TBuilder> module)
    {
        ArgumentGuard.NotNull(module, nameof(module));

        _middlewareModules.Remove(module);
        Debug.WriteLine($"Unregistered middleware module {module.GetType().Name}.");

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
        Debug.WriteLine("Applying service modules...");

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
        Debug.WriteLine("Applying middleware modules...");

        foreach (var module in _middlewareModules)
        {
            module.ConfigureMiddleware(middlewareConfigurator);
        }
        return this;
    }

    /// <inheritdoc />
    public IModuleRegistry<TBuilder> InitializeModules()
    {
        Debug.WriteLine("Initializing modules...");

        foreach (var module in _serviceModules)
        {
            if (module is IInitializable initializable)
            {
                try
                {
                    initializable.Initialize();
                }
                catch
                {
                    throw new AggregateException($"Failed to initialize module {module.GetType().Name}.");
                }
            }
        }

        foreach (var module in _middlewareModules)
        {
            if (module is IInitializable initializable)
            {
                try
                {
                    initializable.Initialize();
                }
                catch
                {
                    throw new AggregateException($"Failed to initialize module {module.GetType().Name}.");
                }
            }
        }

        return this;
    }

    /// <inheritdoc />
    public virtual bool CanHandle<TModule>() where TModule : class, IServiceModule => _serviceModules.OfType<TModule>().Any();

    /// <inheritdoc />
    public virtual bool CanHandle(Type type)
    {
        ArgumentGuard.NotNull(type, nameof(type));

        return _serviceModules.Any(m => m.GetType() == type);
    }
}
