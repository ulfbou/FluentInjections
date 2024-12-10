using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public class ModuleRegistry<TBuilder> : IModuleRegistry<TBuilder>
{
    protected readonly List<IServiceModule> _serviceModules = new();
    protected readonly List<IMiddlewareModule<TBuilder>> _middlewareModules = new();

    public void RegisterConditionalModule<T>(Func<bool> condition) where T : IServiceModule, new()
    {
        if (condition())
        {
            _serviceModules.Add(new T());
        }
    }

    public void RegisterModule(IServiceModule module) => _serviceModules.Add(module);
    public void RegisterModule(IMiddlewareModule<TBuilder> module) => _middlewareModules.Add(module);
    public void RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new() => _serviceModules.Add(new LazyServiceModule<T>(factory, configure));

    public void ApplyServiceModules(IServiceConfigurator serviceConfigurator)
    {
        foreach (var module in _serviceModules)
        {
            module.ConfigureServices(serviceConfigurator);
            (module as IValidatable)?.Validate();
        }
    }

    public void ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator)
    {
        foreach (var module in _middlewareModules)
        {
            module.ConfigureMiddleware(middlewareConfigurator);
            (module as IValidatable)?.Validate();
        }
    }

    public void InitializeModules()
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
    }

    public virtual bool CanHandle<TModule>() where TModule : class, IServiceModule => CanHandle(typeof(TModule));
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
