namespace FluentInjections;

public interface IMiddlewareModule<TBuilder>
{
    void ConfigureMiddleware(IMiddlewareConfigurator<TBuilder> configurator);
}
