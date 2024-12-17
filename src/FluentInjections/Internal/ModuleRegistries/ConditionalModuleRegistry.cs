namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a registry of modules that can be conditionally registered.
/// </summary>
internal class ConditionalModuleRegistry<TBuilder> : ModuleRegistry<TBuilder> where TBuilder : class
{
    /// <summary>
    /// Applies a <see cref="IServiceConfigurator"/> if the 
    /// </summary>
    public void ApplyServicesWithConditions(IServiceProvider serviceProvider, IServiceConfigurator serviceConfigurator)
    {
        foreach (var module in _serviceModules)
        {
            if (module is IConditionalServiceModule conditionalModule && !conditionalModule.ShouldRegister(serviceProvider))
            {
                continue;
            }

            module.ConfigureServices(serviceConfigurator);
        }
    }

    /// <inheritdoc/>
    public override bool CanHandle<TModule>() => typeof(IConditionalServiceModule).IsAssignableFrom(typeof(TModule));
}
