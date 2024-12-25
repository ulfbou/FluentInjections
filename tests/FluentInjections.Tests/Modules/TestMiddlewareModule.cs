using FluentInjections.Tests.Middlewares;

using Microsoft.AspNetCore.Builder;

namespace FluentInjections.Tests.Modules;

public sealed class TestMiddlewareModule() : Module<IMiddlewareConfigurator>(), IMiddlewareModule
{
    public override void Configure(IMiddlewareConfigurator configurator) => configurator.UseMiddleware<TestMiddleware>();
}
