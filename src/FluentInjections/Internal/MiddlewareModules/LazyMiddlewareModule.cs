using Microsoft.AspNetCore.Http;

namespace FluentInjections.Internal.MiddlewareModules;

public class LazyMiddlewareModule<TBuilder, TMiddleware> : IMiddlewareModule<TBuilder> where TMiddleware : class, IMiddleware
{
    public TMiddleware Instance => _lazyInstance.Value;
    private readonly Lazy<TMiddleware> _lazyInstance;

    public LazyMiddlewareModule(Func<TMiddleware> factory)
    {
        _lazyInstance = new Lazy<TMiddleware>(factory);
    }

    public void ConfigureMiddleware(IMiddlewareConfigurator<TBuilder> configurator)
    {
        //configurator.Use<TMiddleware>();
    }
}
