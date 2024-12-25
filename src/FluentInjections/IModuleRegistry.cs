namespace FluentInjections;

/// <summary>
/// Represents a registry for managing service and middleware modules.
/// </summary>
public interface IModuleRegistry
{
    /// <summary>
    /// Registers a component module.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <typeparam name="TConfigurator">The type of the configurator used to configure the module.</typeparam>
    /// <param name="module">The component module to register.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry Register<TModule, TConfigurator>(TModule module)
        where TModule : IModule<TConfigurator>
        where TConfigurator : IConfigurator;

    /// <summary>
    /// Registers a component module.
    /// </summary>
    /// <param name="configuratorType">The type of the configurator used to configure the module.</param>
    /// <param name="module">The component module to register.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry Register<TConfigurator>(Type moduleType, IModule<TConfigurator> module)
        where TConfigurator : IConfigurator;

    /// <summary>
    /// Registers a module using a factory method and an optional configuration action.
    /// </summary>
    /// <typeparam name="TConfigurator">The type of the module.</typeparam>
    /// <param name="factory">The factory method to create the module.</param>
    /// <param name="configure">An optional configuration action for the module.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry Register<TModule, TConfigurator>(Func<TModule> factory, Action<TModule>? configure = null)
        where TModule : IModule<TConfigurator>
        where TConfigurator : IConfigurator;

    /// <summary>
    /// Unregisters a service module.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <param name="module">The service module to unregister.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry Unregister<TModule, TConfigurator>(TModule module)
        where TModule : IModule<TConfigurator>
        where TConfigurator : IConfigurator;

    /// <summary>
    /// Unregisters a service module.
    /// </summary>
    /// <param name="module">The service module to unregister.</param>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry Unregister<TConfigurator>(Type moduleType, IModule<TConfigurator> module)
        where TConfigurator : IConfigurator;

    /// <summary>
    /// Applies all registered component modules.
    /// </summary>
    /// <param name="serviceConfigurator">The service configurator.</param>
    /// <returns>The module registry instance.</returns>

    IModuleRegistry Apply<TConfigurator>(TConfigurator configurator)
        where TConfigurator : IConfigurator;

    /// <summary>
    /// Initializes all registered modules.
    /// </summary>
    /// <returns>The module registry instance.</returns>
    IModuleRegistry Initialize();
}