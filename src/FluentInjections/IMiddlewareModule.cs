namespace FluentInjections;

public interface IMiddlewareModule
{
    void ConfigureMiddleware(IMiddlewareConfigurator configurator);
}
