using FluentInjections;

using Tenants.Services;

namespace Tenants.Modules;

public class ServiceModule : Module<IServiceConfigurator>
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<ITenantService>().To<TenantService>().AsSingleton();
    }
}
