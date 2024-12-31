using FluentInjections;

using Xunit;
using FluentInjections.Tests.Utility.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Units.Configurator;

public abstract class ConfiguratorTests<TConfigurator, TService, TFixture>
    where TConfigurator : class, IConfigurator
    where TService : class
    where TFixture : class, IConfiguratorFixture<TConfigurator, TService>, new()
{
    protected TFixture Fixture { get; set; }
    protected TService Services { get; set; }
    protected TConfigurator Configurator { get; set; }
    protected IServiceProvider? Provider { get; set; }

    protected ConfiguratorTests()
    {
        Fixture = new TFixture();
        Fixture.Setup();

        Services = Fixture.Services;
        Configurator = Fixture.Configurator;
    }

    protected virtual T? GetService<T>() where T : class
    {
        if (Provider is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetService<T>.");
        }

        return Provider.GetService<T>();
    }

    protected virtual T GetRequiredService<T>() where T : notnull
    {
        if (Provider is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredService<T>.");
        }

        return Provider.GetRequiredService<T>();
    }

    protected virtual object? GetRequiredNamedService<T>(string name) where T : notnull
    {
        if (Provider is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredNamedService<T>.");
        }

        return Provider.GetNamedService<T>(name);
    }
}
