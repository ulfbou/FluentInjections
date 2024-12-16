namespace FluentInjections.Internal.ServiceModules;

public class LazyServiceModule<T> : IServiceModule where T : class, new()
{
    public T Instance => _lazyInstance.Value;
    private readonly Lazy<T> _lazyInstance;

    public LazyServiceModule(Func<T> factory, Action<T>? configure = null!)
    {
        _lazyInstance = new Lazy<T>(() =>
        {
            var instance = factory();
            configure?.Invoke(instance);

            return instance;
        });
    }

    public void ConfigureServices(IServiceConfigurator configurator)
    {
        //configurator.AddSingleton(Instance);
    }
}
