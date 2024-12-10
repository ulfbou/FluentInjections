using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public class LazyMiddlewareModule<T> : IMiddlewareModule where T : class, IMiddleware
{
    public T Instance => _lazyInstance.Value;
    private readonly Lazy<T> _lazyInstance;

    public LazyMiddlewareModule(Func<T> factory)
    {
        _lazyInstance = new Lazy<T>(factory);
    }

    public void ConfigureMiddleware(IMiddlewareConfigurator configurator)
    {
        configurator.Use<T>();
    }
}
