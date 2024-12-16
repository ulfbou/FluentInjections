namespace FluentInjections.Internal.Modules;

internal class LazyServiceModule : IServiceModule, IInitializable
{
    private readonly Lazy<IServiceModule> _lazyModule;

    public LazyServiceModule(Func<IServiceModule> moduleFactory)
    {
        _lazyModule = new Lazy<IServiceModule>(moduleFactory);
    }

    public void ConfigureServices(IServiceConfigurator configurator)
    {
        _lazyModule.Value.ConfigureServices(configurator);
    }

    public void Initialize()
    {
        if (_lazyModule.IsValueCreated)
        {
            (_lazyModule.Value as IInitializable)?.Initialize();
        }
    }
}
