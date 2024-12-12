using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public interface IMiddlewareConfigurator<TBuilder>
{
    IMiddlewareConfigurator<TBuilder> Use<TMiddleware>(params object[] args) where TMiddleware : class, IMiddleware;
    IMiddlewareConfigurator<TBuilder> Use(Type middlewareType, params object?[] args);
    TBuilder Builder { get; }
}
