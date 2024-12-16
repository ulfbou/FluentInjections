using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public interface IMiddlewareConfigurator<TBuilder>
{
    IMiddlewareBinding<TMiddleware, TBuilder> Bind<TMiddleware>() where TMiddleware : class;
    TBuilder Builder { get; }
}
