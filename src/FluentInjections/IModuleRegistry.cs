namespace FluentInjections;

public interface IModuleRegistry<TBuilder> where TBuilder : class
{
    /// <summary>
    /// Registers a service module with a condition.
    /// </summary>
    /// <typeparam name="T">The module type.</typeparam>
    /// <param name="condition">The condition to determine if the module should be registered.</param>
    /// <returns>The module registry.</returns>
    IModuleRegistry<TBuilder> RegisterModule(IServiceModule module);

    /// <summary>
    /// Registers a middleware module.
    /// </summary>
    /// <param name="module">The middleware module.</param>
    /// <returns>The module registry.</returns>
    IModuleRegistry<TBuilder> RegisterModule(IMiddlewareModule<TBuilder> module);

    /// <summary>
    /// Registers a module with a condition.
    /// </summary>
    /// <typeparam name="T">The module type.</typeparam>
    /// <param name="condition">The condition to determine if the module should be registered.</param>
    /// <returns>The module registry.</returns>
    IModuleRegistry<TBuilder> RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new();

    /// <summary>
    /// Registers a module with a condition.
    /// </summary>
    /// <typeparam name="T">The module type.</typeparam>
    /// <param name="condition">The condition to determine if the module should be registered.</param>
    /// <returns>The module registry.</returns>
    IModuleRegistry<TBuilder> RegisterModule<T>(Func<bool> condition) where T : IServiceModule, new();

    /// <summary>
    /// Applies the service modules.
    /// </summary>
    /// <param name="serviceConfigurator">The service configurator.</param>
    /// <returns>The module registry.</returns>
    IModuleRegistry<TBuilder> ApplyServiceModules(IServiceConfigurator serviceConfigurator);

    /// <summary>
    /// Applies the middleware modules.
    /// </summary>
    /// <param name="middlewareConfigurator">The middleware configurator.</param>
    /// <returns>The module registry.</returns>
    IModuleRegistry<TBuilder> ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator);

    /// <summary>
    /// Initializes the modules.
    /// </summary>
    /// <returns>The module registry.</returns>
    IModuleRegistry<TBuilder> InitializeModules();

    /// <summary>
    /// Checks if the module registry can handle the specified module.
    /// </summary>
    /// <typeparam name="TModule">The module type.</typeparam>
    /// <returns>True, if the module registry can handle the specified module; otherwise, false.</returns>
    bool CanHandle<TModule>() where TModule : class, IServiceModule;

    /// <summary>
    /// Checks if the module registry can handle the specified module.
    /// </summary>
    /// <param name="type">The module type.</param>
    /// <returns>True, if the module registry can handle the specified module; otherwise, false.</returns>
    bool CanHandle(Type type);
}
