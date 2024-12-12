namespace FluentInjections;

/// <summary>
/// Represents a registry of modules that implement the <see cref="IModuleLifecycleHook"/> interface.
/// </summary>
/// <typeparam name="TBuilder">The builder type.</typeparam>
public class LifecycleModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
    /// <inheritdoc/>
    public override bool CanHandle<TModule>() => typeof(IModuleLifecycleHook).IsAssignableFrom(typeof(TModule));

    /// <summary>
    /// Initializes the modules.
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
    /// Terminates the modules.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public void TerminateModules(IServiceProvider serviceProvider)
    {
        foreach (var module in _serviceModules.OfType<IModuleLifecycleHook>().Reverse())
        {
            module.OnShutdown(serviceProvider);
        }
    }
}
