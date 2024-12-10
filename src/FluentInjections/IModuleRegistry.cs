
namespace FluentInjections;

public interface IModuleRegistry<TBuilder>
{
    void RegisterModule(IServiceModule module);
    void RegisterModule(IMiddlewareModule<TBuilder> module);
    void RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new();
    void ApplyServiceModules(IServiceConfigurator serviceConfigurator);
    void ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator);
    void InitializeModules();
    void RegisterConditionalModule<T>(Func<bool> condition) where T : IServiceModule, new();
}
