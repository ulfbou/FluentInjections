namespace FluentInjections;

public class PrioritizedModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
    public void ApplyServicesWithPriority(IServiceConfigurator serviceConfigurator)
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
    }
}
