namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a module registry that can handle validated services.
/// </summary>
/// <typeparam name="TBuilder">The builder type.</typeparam>
internal class ValidationEnabledModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
    /// <summary>
    /// Applies the validated services.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="serviceConfigurator">The service configurator.</param>
    public IModuleRegistry<TBuilder> ApplyValidatedServices(IServiceProvider serviceProvider, IServiceConfigurator serviceConfigurator)
    {
        foreach (var module in _serviceModules)
        {
            if (module is IValidatableServiceModule validatableModule)
            {
                validatableModule.Validate(serviceProvider);
            }

            module.ConfigureServices(serviceConfigurator);
        }

        return this;
    }

    public override bool CanHandle<TModule>() => typeof(IValidatableServiceModule).IsAssignableFrom(typeof(TModule));
}
