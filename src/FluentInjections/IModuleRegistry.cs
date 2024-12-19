namespace FluentInjections;

/// <summary>
/// Represents a registry for managing service and middleware modules.
/// </summary>
/// <typeparam name="TBuilder">The type of the application builder.</typeparam>
public interface IModuleRegistry<TBuilder> where TBuilder : class
{
    /// <summary>
    /// Registers a service module.
    /// </summary>
    /// <param name="module">The service module to register.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> RegisterModule(IServiceModule module);

    /// <summary>
    /// Unregisters a service module.
    /// </summary>
    /// <param name="module">The service module to unregister.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> UnregisterModule(IServiceModule module);

    /// <summary>
    /// Registers a middleware module.
    /// </summary>
    /// <param name="module">The middleware module to register.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> RegisterModule(IMiddlewareModule<TBuilder> module);

    /// <summary>
    /// Unregisters a middleware module.
    /// </summary>
    /// <param name="module">The middleware module to unregister.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> UnregisterModule(IMiddlewareModule<TBuilder> module);

    /// <summary>
    /// Registers a module using a factory method and an optional configuration action.
    /// </summary>
    /// <typeparam name="T">The type of the module.</typeparam>
    /// <param name="factory">The factory method to create the module.</param>
    /// <param name="configure">An optional configuration action for the module.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> RegisterModule<T>(Func<T> factory, Action<T>? configure = null) where T : class, new();

    /// <summary>
    /// Registers a module based on a condition.
    /// </summary>
    /// <typeparam name="T">The type of the module.</typeparam>
    /// <param name="condition">The condition to determine if the module should be registered.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> RegisterModule<T>(Func<bool> condition) where T : IServiceModule, new();

    /// <summary>
    /// Applies all registered service modules.
    /// </summary>
    /// <param name="serviceConfigurator">The service configurator.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> ApplyServiceModules(IServiceConfigurator serviceConfigurator);

    /// <summary>
    /// Applies all registered middleware modules.
    /// </summary>
    /// <param name="middlewareConfigurator">The middleware configurator.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> ApplyMiddlewareModules(IMiddlewareConfigurator<TBuilder> middlewareConfigurator);

    /// <summary>
    /// Initializes all registered modules.
    /// </summary>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry<TBuilder> InitializeModules();

    /// <summary>
    /// Determines if the module registry can handle a specific type of module.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <returns>True if the module registry can handle the specified module, otherwise false.</returns>
    bool CanHandle<TModule>() where TModule : class, IServiceModule;

    /// <summary>
    /// Determines if the module registry can handle a specific type of module.
    /// </summary>
    /// <param name="type">The type of the module.</param>
    /// <returns>True if the module registry can handle the specified module, otherwise false.</returns>
    bool CanHandle(Type type);
}