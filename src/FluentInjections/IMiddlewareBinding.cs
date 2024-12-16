using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public interface IMiddlewareBinding<TMiddleware, TBuilder> where TMiddleware : class
{
    IMiddlewareBinding<TMiddleware, TBuilder> To<TImplementation>() where TImplementation : class, TMiddleware;
    IMiddlewareBinding<TMiddleware, TBuilder> WithParameters(object parameters);
    IMiddlewareBinding<TMiddleware, TBuilder> WithLifetime(ServiceLifetime lifetime);
    IMiddlewareBinding<TMiddleware, TBuilder> Configure(Action<TMiddleware> configure);
    IMiddlewareBinding<TMiddleware, TBuilder> ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class;
    IMiddlewareBinding<TMiddleware, TBuilder> ConfigureOptions<TOptions>(Action<TMiddleware, TOptions> configure) where TOptions : class;
    IMiddlewareBinding<TMiddleware, TBuilder> ConfigureOptions<TOptions>(Action<TMiddleware, TOptions, IMiddlewareConfigurator<TBuilder>> configure) where TOptions : class;
    void Register();
}
