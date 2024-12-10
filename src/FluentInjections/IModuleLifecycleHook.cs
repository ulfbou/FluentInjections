namespace FluentInjections;

public interface IModuleLifecycleHook
{
    void OnStartup(IServiceProvider serviceProvider);
    void OnShutdown(IServiceProvider serviceProvider);
}
