using FluentInjections;

using Xunit;
using FluentInjections.Tests.Utility.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Tests.Internal.Services;
using Moq;

namespace FluentInjections.Tests.Units.Configurator;

public abstract class ConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    where TConfigurator : class, IConfigurator
    where TServices : class, IServiceCollection
    where TProvider : class, IServiceProvider
    where TFixture : class, IConfiguratorFixture<TConfigurator, TServices, TProvider>, new()
{
    internal TFixture Fixture { get; set; }
    internal TServices Services { get; set; }
    internal abstract TConfigurator Configurator { get; set; }
    internal abstract TProvider? Provider { get; set; }
    public Mock<ITestService> MockTestService { get; set; }

    public ConfiguratorTests()
    {
        Fixture = new();
        Services = Fixture.Services;
        Configurator = Fixture.Configurator;
        Provider = Fixture.Provider;
        MockTestService = Fixture.MockTestService;
    }

    [Fact]
    public void SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode()
    {
        // Arrange
        var configurator = Configurator;

        // Act
        void action() => configurator.ConflictResolution = ((ConflictResolutionMode)int.MaxValue);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    internal abstract void BuildProvider();

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
