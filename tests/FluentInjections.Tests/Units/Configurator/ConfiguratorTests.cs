using FluentInjections;

using Xunit;
using FluentInjections.Tests.Utility.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Tests.Internal.Services;
using Moq;
using FluentInjections.Validation;

namespace FluentInjections.Tests.Units.Configurator;

/// <summary>
/// Represents a base class for testing configurators.
/// </summary>
/// <typeparam name="TConfigurator">The type of the configurator.</typeparam>
/// <typeparam name="TServices">The type of the services collection.</typeparam>
/// <typeparam name="TProvider">The type of the service provider.</typeparam>
/// <typeparam name="TFixture">The type of the configurator fixture.</typeparam>
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
        Provider = Fixture.Provider;
        MockTestService = Fixture.TestServiceMock;
    }

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

    /// <summary>
    /// Gets a service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service to get.</typeparam>
    protected virtual T? GetService<T>() where T : class
    {
        if (Provider is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetService<T>.");
        }

        return Provider.GetService<T>();
    }

    /// <summary>
    /// Gets a required service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service to get.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service provider is not built.</exception>
    protected virtual T GetRequiredService<T>() where T : notnull
    {
        if (Provider is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredService<T>.");
        }

        return Provider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a named service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service to get.</typeparam>
    /// <param name="name">The name of the service to get.</param>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service provider is not built.</exception>
    protected virtual object? GetRequiredNamedService<T>(string name) where T : notnull
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));

        if (Provider is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredNamedService<T>.");
        }

        return Provider.GetNamedService<T>(name);
    }
}
