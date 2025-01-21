using FluentInjections.Tests.Internal.Middlewares;

using Microsoft.AspNetCore.Builder;

using Moq;

namespace FluentInjections.Tests.Internal.Modules;

public class TestMiddlewareModule : Module<IMiddlewareConfigurator>
{
    internal TestMiddlewareModule() : base(Mock.Of<IApplicationBuilder>()) { }

    public override void Configure(IMiddlewareConfigurator configurator)
    {
        configurator.UseMiddleware<TestMiddleware>();
    }
}
