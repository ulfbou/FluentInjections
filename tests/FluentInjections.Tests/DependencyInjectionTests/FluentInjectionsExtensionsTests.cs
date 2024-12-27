using FluentAssertions.Common;
using FluentInjections.Tests.Services;
using FluentInjections.Tests;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using FluentInjections.Tests.Modules;
using System.Configuration;
using FluentInjections.Internal.Configurators;

namespace FluentInjections.Tests.DependencyInjectionTests;

public class FluentInjectionsExtensionsServiceTests : BaseConfiguratorTest<IServiceConfigurator, IServiceBinding>
{
    public FluentInjectionsExtensionsServiceTests() : base()
    {
        Services.AddFluentInjections(typeof(NamedTestServiceModule).Assembly);
        Configurator = new ServiceConfigurator(Builder);
    }
    // Test that TestService is registered correctly: Resolves with no name and has the correct parameters set.
    [Fact]
    public void AddFluentInjections_ShouldRegisterTestService_WithDefaultValues()
    {
        // Arrange
        Register<ITestService>(binding =>
        {
            binding.To<TestServiceWithDefaultValues>();
        });

        // Act
        Register();
        var testService = Resolve<ITestService>();

        // Assert
        testService.Should().NotBeNull();
        testService.Should().BeOfType<TestServiceWithDefaultValues>();
        testService.As<TestServiceWithDefaultValues>().Param1.Should().Be("default");
        testService.As<TestServiceWithDefaultValues>().Param2.Should().Be(-1);
    }

    // Test that Test42 is registered correctly: Resolves with name Test42 and has the correct parameters set.
    [Fact]
    public void AddFluentInjections_ShouldRegisterNamedTestService_WithCorrectParameters()
    {
        // Arrange & Act
        var namedService = Resolve<ITestService>("Test42");

        // Assert
        namedService.Should().NotBeNull();
        namedService.Should().BeOfType<TestService>();
        namedService.As<TestService>().Param1.Should().Be("value1");
        namedService.As<TestService>().Param2.Should().Be(42);
    }

    // Test that Test43 is registered correctly: Resolves with name Test43. 
    [Fact]
    public void AddFluentInjections_ShouldRegisterNamedTestService()
    {
        // Arrange & Act
        var namedService = Resolve<ITestService>("Test43");

        // Assert
        namedService.Should().NotBeNull();
    }

    [Fact]
    public void AddFluentInjections_ShouldRegisterServicesAndMiddleware()
    {
        // Arrange & Act
        var testService = Resolve<ITestService>();

        // Assert
        testService.Should().NotBeNull();
        testService.Should().BeOfType<TestServiceWithDefaultValues>();
    }


    protected void Register<TService>(Action<IServiceBinding<TService>> configure) where TService : notnull
    {
        var binding = Configurator.Bind<TService>();
        configure(binding);
    }
}
