namespace FluentInjections;

public class ConditionalModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
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
}
