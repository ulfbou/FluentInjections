namespace FluentInjections.Internal.ServiceModules;

public abstract class LifecycleAwareServiceModule : IServiceModule, IModuleLifecycleHook
{
    public abstract void ConfigureServices(IServiceConfigurator configurator);

    public virtual void OnStartup(IServiceProvider serviceProvider)
    {
        // Optional: Override in derived classes for startup logic
    }

    public virtual void OnShutdown(IServiceProvider serviceProvider)
    {
        // Optional: Override in derived classes for shutdown logic
    }
}
