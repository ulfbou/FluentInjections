using FluentInjections.Constants;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;

namespace FluentInjections;

public class CompositeModuleRegistry<TBuilder> : IModuleRegistry<TBuilder>
{
    private readonly ConcurrentBag<IModuleRegistry<TBuilder>> _registries = new();
    private readonly IServiceProvider _serviceProvider;

    public CompositeModuleRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void AddRegistry(IModuleRegistry<TBuilder> registry)
    {
        _registries.Add(registry);
    }

    public void RegisterModule(IServiceModule module)
    {
        if (!TryRegisterModule(module.GetType(), registry => registry.RegisterModule(module)))
        {
            var moduleRegistry = _serviceProvider.GetRequiredService<ModuleRegistry<TBuilder>>();
            moduleRegistry.RegisterModule(module);
        }
    }

    public void RegisterModule(IMiddlewareModule<TBuilder> module)
    {
        if (!TryRegisterModule(module.GetType(), registry => registry.RegisterModule(module)))
        {
            var moduleRegistry = _serviceProvider.GetRequiredService<ModuleRegistry<TBuilder>>();
            moduleRegistry.RegisterModule(module);
        }
    }

    public void RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new()
    {
        if (!TryRegisterModule(typeof(T), registry => registry.RegisterModule(factory, configure)))
        {
            var moduleRegistry = _serviceProvider.GetRequiredService<ModuleRegistry<TBuilder>>();
            moduleRegistry.RegisterModule(factory, configure);
        }
    }

    public void RegisterConditionalModule<T>(Func<bool> condition) where T : IServiceModule, new()
    {
        if (!TryRegisterModule(typeof(T), registry => registry.RegisterConditionalModule<T>(condition)))
        {
            throw new InvalidRegistrationException($"No registry was found that can handle the module type {typeof(T).Name}.");
        }
    }

    public void ApplyServiceModules(IServiceConfigurator serviceConfigurator)
    {
        Parallel.ForEach(_registries, registry => registry.ApplyServiceModules(serviceConfigurator));
    }

    public void ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator)
    {
        Parallel.ForEach(_registries, registry => registry.ApplyMiddlewareModules(middlewareConfigurator));
    }

    public void InitializeModules()
    {
        Parallel.ForEach(_registries, registry => registry.InitializeModules());
    }

    public bool CanHandle(Type moduleType)
        => throw new InvalidRegistrationException(ErrorMessages.Composite.CallCanHandle);

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
