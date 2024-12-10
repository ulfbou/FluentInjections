using System.Net.Http;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public class MiddlewareConfigurator : IMiddlewareConfigurator
{
    private IApplicationBuilder _app;

    public MiddlewareConfigurator(IApplicationBuilder app)
    {
        _app = app;
    }

    public IMiddlewareConfigurator Use<TMiddleware>() where TMiddleware : class, IMiddleware
    {
        _app.UseMiddleware<TMiddleware>();
        return this;
    }
}
