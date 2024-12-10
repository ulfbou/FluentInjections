using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public class ModuleRegistry : IModuleRegistry
{
    protected readonly List<IServiceModule> _serviceModules = new();
    protected readonly List<IMiddlewareModule> _middlewareModules = new();

    public void RegisterModule(IServiceModule module) => _serviceModules.Add(module);
    public void RegisterModule(IMiddlewareModule module) => _middlewareModules.Add(module);

    public void RegisterConditionalModule<T>(Func<bool> condition) where T : IServiceModule, new()
    {
        if (condition())
        {
            _serviceModules.Add(new T());
        }
    }

    public void RegisterServiceModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new()
    {
        _serviceModules.Add(new LazyServiceModule<T>(factory, configure));
    }

    public void ApplyServiceModules(IServiceConfigurator serviceConfigurator)
    {
        foreach (var module in _serviceModules)
        {
            module.ConfigureServices(serviceConfigurator);
            (module as IValidatable)?.Validate();
        }
    }

    public void ApplyMiddlewareModules(IMiddlewareConfigurator middlewareConfigurator)
    {
        foreach (var module in _middlewareModules)
        {
            module.ConfigureMiddleware(middlewareConfigurator);
            (module as IValidatable)?.Validate();
        }
    }

    public void InitializeAllModules()
    {
        foreach (var module in _serviceModules.OfType<ILifecycleModule>())
        {
            module.Initialize();
        }
    }
}
