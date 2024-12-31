using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Units.Configurator;

public sealed class AutofacServiceConfiguratorTests : ServiceConfiguratorTests<AutofacServiceConfigurator, ContainerBuilder, AutofacServiceConfiguratorFixture>
{
    IContainer? Container { get; set; }
    IServiceCollection ServiceCollection { get; set; }

    public AutofacServiceConfiguratorTests() : base()
    {
        ServiceCollection = new ServiceCollection();
    }

    protected override void BuildProvider()
    {
        if (Container is not null)
        {
            throw new InvalidOperationException("Container has already been built.");
        }

        Services.Populate(ServiceCollection);
        Container = Services.Build();
        Provider = new AutofacServiceProvider(Container);
    }

    protected override T? GetService<T>() where T : class
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built. Ensure that BuildProvider is called prior to calling GetService<T>.");
        }

        return Container.Resolve<T>();
    }

    protected override T GetRequiredService<T>()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built. Ensure that BuildProvider is called prior to calling GetRequiredService<T>.");
        }

        return Container.Resolve<T>();
    }

    protected override object? GetRequiredNamedService<T>(string name)
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built. Ensure that BuildProvider is called prior to calling GetRequiredNamedService<T>.");
        }

        return Container.ResolveKeyed<T>(name);
    }
}
