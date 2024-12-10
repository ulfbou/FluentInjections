using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public interface IMiddlewareConfigurator
{
    IMiddlewareConfigurator Use<TMiddleware>() where TMiddleware : class, IMiddleware;
}
