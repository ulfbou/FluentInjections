namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a registry of modules that can be prioritized.
/// </summary>
/// <typeparam name="TBuilder">The builder type.</typeparam>
internal class PrioritizedModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
    /// <summary>
    /// Applies the services with priority.
    /// </summary>
    public IModuleRegistry<TBuilder> ApplyServicesWithPriority(IServiceConfigurator serviceConfigurator)
    {
        foreach (var module in _serviceModules
                     .OfType<IPrioritizedServiceModule>()
                     .OrderBy(m => m.Priority))
        {
            module.ConfigureServices(serviceConfigurator);
        }

        foreach (var module in _serviceModules.Except(_serviceModules.OfType<IPrioritizedServiceModule>()))
        {
            module.ConfigureServices(serviceConfigurator);
        }

        return this;
    }

    /// <summary>
    /// Applies the middleware with priority.
    /// </summary>
    /// <param name="TModule">The module type.</param>
    public override bool CanHandle<TModule>() => typeof(IPrioritizedServiceModule).IsAssignableFrom(typeof(TModule));
}
