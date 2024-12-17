namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a registry of modules that implement the <see cref="IModuleLifecycleHook"/> interface.
/// </summary>
/// <typeparam name="TBuilder">The builder type.</typeparam>
internal class LifecycleModuleRegistry<TBuilder> : ModuleRegistry<TBuilder> where TBuilder : class
{
    /// <summary>
    /// Initializes the lifecycle modules.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public void InitializeModules(IServiceProvider serviceProvider)
    {
        foreach (var module in _serviceModules.OfType<IModuleLifecycleHook>())
        {
            module.OnStartup(serviceProvider);
        }
    }

    /// <summary>
    /// Terminates the lifecycle modules.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public void TerminateModules(IServiceProvider serviceProvider)
    {
        foreach (var module in _serviceModules.OfType<IModuleLifecycleHook>().Reverse())
        {
            module.OnShutdown(serviceProvider);
        }
    }

    /// <inheritdoc/>
    public override bool CanHandle<TModule>() => typeof(IModuleLifecycleHook).IsAssignableFrom(typeof(TModule));
}
