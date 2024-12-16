using Microsoft.AspNetCore.Http;

namespace FluentInjections.Internal.Modules;

internal class LazyMiddlewareModule<TBuilder> : IMiddlewareModule<TBuilder>, IInitializable
{
    private readonly Lazy<IMiddlewareModule<TBuilder>> _lazyModule;

    public LazyMiddlewareModule(Func<IMiddlewareModule<TBuilder>> moduleFactory)
    {
        _lazyModule = new Lazy<IMiddlewareModule<TBuilder>>(moduleFactory);
    }

    public void ConfigureMiddleware(IMiddlewareConfigurator<TBuilder> configurator)
    {
        _lazyModule.Value.ConfigureMiddleware(configurator);
    }

    public void Initialize()
    {
        if (_lazyModule.IsValueCreated)
        {
            (_lazyModule.Value as IInitializable)?.Initialize();
        }
    }
}
