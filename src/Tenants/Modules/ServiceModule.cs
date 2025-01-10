using FluentInjections;

using Microsoft.OpenApi.Models;

using Tenants.Middleware;
using Tenants.Services;

namespace Tenants.Modules;

public class ServiceModule : Module<IServiceConfigurator>
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<ITenantService>().To<TenantService>().AsSingleton();
        configurator.Bind<TenantMiddleware>()
                    .WithMetadata("Name", "TenantMiddleware")
                    .WithName("TenantMiddleware")
                    .AsSingleton()
                    .AsSelf();
        configurator.Services
                    .AddControllers();
        configurator.Services
                    .AddEndpointsApiExplorer();
        configurator.Services
                     .AddOpenApi();
    }
}
