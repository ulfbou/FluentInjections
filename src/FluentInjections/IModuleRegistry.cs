namespace FluentInjections;

public interface IModuleRegistry<TBuilder>
{
    IModuleRegistry<TBuilder> RegisterModule(IServiceModule module);
    IModuleRegistry<TBuilder> RegisterModule(IMiddlewareModule<TBuilder> module);
    IModuleRegistry<TBuilder> RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new();
    IModuleRegistry<TBuilder> RegisterModule<T>(Func<bool> condition) where T : IServiceModule, new();
    IModuleRegistry<TBuilder> ApplyServiceModules(IServiceConfigurator serviceConfigurator);
    IModuleRegistry<TBuilder> ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator);
    IModuleRegistry<TBuilder> InitializeModules();

    bool CanHandle<TModule>() where TModule : class, IServiceModule;
    bool CanHandle(Type type);
}
