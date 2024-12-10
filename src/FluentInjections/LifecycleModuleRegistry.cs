namespace FluentInjections;

public class LifecycleModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
    public override bool CanHandle<TModule>() => typeof(IModuleLifecycleHook).IsAssignableFrom(typeof(TModule));

    public void InitializeModules(IServiceProvider serviceProvider)
    {
        foreach (var module in _serviceModules.OfType<IModuleLifecycleHook>())
        {
            module.OnStartup(serviceProvider);
        }
    }

    public void TerminateModules(IServiceProvider serviceProvider)
    {
        foreach (var module in _serviceModules.OfType<IModuleLifecycleHook>().Reverse())
        {
            module.OnShutdown(serviceProvider);
        }
    }
}
