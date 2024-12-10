using System.Net.Http;

using Autofac;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentInjections;

public class MiddlewareConfigurator<TBuilder> : IMiddlewareConfigurator<TBuilder>
{
    public TBuilder Builder => _builder;
    private TBuilder _builder;

    public MiddlewareConfigurator(TBuilder builder)
    {
        _builder = builder;
    }

    public IMiddlewareConfigurator<TBuilder> Use<TMiddleware>(params object?[] args) where TMiddleware : class, IMiddleware => Use(typeof(TMiddleware), args);

    public IMiddlewareConfigurator<TBuilder> Use(Type middlewareType, params object?[] args)
    {
        if (!typeof(IMiddleware).IsAssignableFrom(middlewareType))
        {
            throw new ArgumentException($"The middleware type {middlewareType.Name} does not implement IMiddleware.");
        }

        if (Builder is IApplicationBuilder app)
        {
            app.UseMiddleware(middlewareType, args);
        }
        else if (Builder is WebApplication webApp)
        {
            webApp.UseMiddleware(middlewareType, args);
        }
        else
        {
            throw new NotSupportedException($"Builder type {Builder!.GetType().Name} is not supported.");
        }

        return this;
    }
}
