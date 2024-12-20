using FluentInjections.Tests.Middlewares;

using Microsoft.AspNetCore.Builder;

namespace FluentInjections.Tests.Modules;

public sealed class TestMiddlewareModule : IMiddlewareModule<WebApplicationBuilder>
{
    public void ConfigureMiddleware(IMiddlewareConfigurator<WebApplicationBuilder> configurator)
    {
        configurator.UseMiddleware<TestMiddleware>();
    }
}
