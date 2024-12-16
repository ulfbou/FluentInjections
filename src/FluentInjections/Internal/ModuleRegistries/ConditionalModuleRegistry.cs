namespace FluentInjections.Internal.Registries;

internal class ConditionalModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
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

    public override bool CanHandle<TModule>() => typeof(IConditionalServiceModule).IsAssignableFrom(typeof(TModule));
}
