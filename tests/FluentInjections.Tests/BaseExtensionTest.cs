using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests;

public abstract class BaseExtensionTest
{
    protected IServiceCollection Services { get; }
    protected ContainerBuilder Builder { get; }
    protected IContainer Container { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }


    protected BaseExtensionTest()
    {
        Services = new ServiceCollection();
        Builder = new ContainerBuilder();
        Container = default!;
        ServiceProvider = default!;
    }

    protected void Register()
    {
        Builder.Populate(Services);
        Container = Builder.Build();
        ServiceProvider = new AutofacServiceProvider(Container);
    }

    protected T? Resolve<T>() where T : notnull
        => Container.Resolve<T>();

    protected T? Resolve<T>(string name) where T : notnull
        => Container.ResolveNamed<T>(name);

    protected ILifetimeScope CreateScope()
        => Container.BeginLifetimeScope();
}
