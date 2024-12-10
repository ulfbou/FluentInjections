namespace FluentInjections;

public class ValidationEnabledModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
    public void ApplyValidatedServices(IServiceProvider serviceProvider, IServiceConfigurator serviceConfigurator)
    {
        foreach (var module in _serviceModules)
        {
            if (module is IValidatableServiceModule validatableModule)
            {
                validatableModule.Validate(serviceProvider);
            }

            module.ConfigureServices(serviceConfigurator);
        }
    }

    public override bool CanHandle<TModule>() => typeof(IValidatableServiceModule).IsAssignableFrom(typeof(TModule));
}
