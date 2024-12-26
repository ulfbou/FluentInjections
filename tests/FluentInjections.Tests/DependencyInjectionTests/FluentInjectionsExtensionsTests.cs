using FluentAssertions.Common;
using FluentInjections.Tests.Services;
using FluentInjections.Tests;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace FluentInjections.Tests.DependencyInjectionTests;

public class FluentInjectionsExtensionsTests : BaseExtensionTest
{
    [Fact]
    public void AddFluentInjections_ShouldRegisterNamedServices()
    {
        // Arrange
        var assemblies = new[] { typeof(TestService).Assembly };

        // Act
        Services.AddFluentInjections(assemblies);
        Register();
        var namedService = Resolve<ITestService>("Test43");

        // Assert
        namedService.Should().NotBeNull();
    }

    [Fact]
    public void AddFluentInjections_ShouldRegisterServicesAndMiddleware()
    {
        // Arrange
        var assemblies = new[] { typeof(TestService).Assembly };

        // Act
        Services.AddFluentInjections(assemblies);
        Register();

        // Assert
        var testService = Resolve<ITestService>();
        testService.Should().NotBeNull();
        testService.Should().BeOfType<TestServiceWithDefaultValues>();
    }
}
