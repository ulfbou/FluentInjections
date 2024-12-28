using FluentInjections;

using Tenants.Middleware;

namespace Tenants.Modules;
public class MiddlewareModule : Module<IMiddlewareConfigurator>
{
    public override void Configure(IMiddlewareConfigurator configurator)
    {
        configurator.UseMiddleware<TenantMiddleware>();
    }
}