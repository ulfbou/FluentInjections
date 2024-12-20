namespace FluentInjections.Internal.Modules;

public class LazyServiceModule : IServiceModule, IInitializable
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
public class ServiceModule : IServiceModule, IInitializable
{
    private readonly Action<IServiceConfigurator> _configureServices;
    private readonly Action _initialize;
    public ServiceModule(Action<IServiceConfigurator> configureServices, Action initialize)
    {
        _configureServices = configureServices;
        _initialize = initialize;
    }
    public void ConfigureServices(IServiceConfigurator configurator)
    {
        _configureServices(configurator);
    }
    public void Initialize()
    {
        _initialize();
    }
}
