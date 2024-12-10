
namespace FluentInjections;

public interface IModuleRegistry
{
    void RegisterModule(IServiceModule module);
    void RegisterModule(IMiddlewareModule module);
    void ApplyServiceModules(IServiceConfigurator serviceConfigurator);
    void ApplyMiddlewareModules(IMiddlewareConfigurator middlewareConfigurator);
    void RegisterServiceModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new();
}
