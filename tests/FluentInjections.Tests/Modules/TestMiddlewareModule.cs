using FluentInjections.Tests.Middlewares;

using Microsoft.AspNetCore.Builder;

namespace FluentInjections.Tests.Modules;

public sealed class TestMiddlewareModule : IMiddlewareModule<IApplicationBuilder>
{
    public void ConfigureMiddleware(IMiddlewareConfigurator<IApplicationBuilder> configurator)
    {
        configurator.UseMiddleware<TestMiddleware>();
    }
}
