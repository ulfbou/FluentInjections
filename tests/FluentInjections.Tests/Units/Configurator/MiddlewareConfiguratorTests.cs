using FluentAssertions;
using FluentAssertions.Common;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection;

namespace FluentInjections.Tests.Units.Configurator;
#if false
public abstract partial class MiddlewareConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    : ConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    where TConfigurator : class, IConfigurator
    where TServices : class, IServiceCollection
    where TProvider : class, IServiceProvider
    where TFixture : class, IMiddlewareConfiguratorFixture, IConfiguratorFixture<TConfigurator, TServices, TProvider>, new()
{
    [Fact]
    public void Constructor_ShouldInitializeWithLogger()
    {
        // Arrange
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;

        // Act & Assert
        configurator?.Logger.Should().NotBeNull();
    }

    [Fact]
    public void UseMiddleware_ShouldAddMiddlewareDescriptor()
    {
        // Arrange
        var binding = Configurator.UseMiddleware<TestMiddleware>();
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;

        // Act & Assert
        configurator?.Descriptors.Should().Contain(binding.Descriptor);
    }

    [Fact]
    public void RemoveMiddleware_ShouldRemoveMiddlewareDescriptor()
    {
        // Arrange
        var binding = Configurator.UseMiddleware<TestMiddleware>();
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;

        // Act 
        Configurator.RemoveMiddleware<TestMiddleware>(binding.Descriptor);

        // Assert
        configurator?.Descriptors.Should().NotContain(binding.Descriptor);
    }

    [Fact]
    public void ApplyGroupPolicy_ShouldApplyConfigurationToGroup()
    {
        // Arrange
        var middlewareBinding1 = Configurator.UseMiddleware<MiddlewareA>().InGroup("TestGroup");
        var middlewareBinding2 = Configurator.UseMiddleware<MiddlewareB>().InGroup("TestGroup");
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;

        // Act
        Configurator.ApplyGroupPolicy("TestGroup", binding => binding.Descriptor.Condition = () => false);

        // Assert
        Assert.False(middlewareBinding1.Descriptor.IsEnabled);
        Assert.False(middlewareBinding2.Descriptor.IsEnabled);
    }

    [Fact]
    public void Register_ShouldInvokeRegisterMethodForEachDescriptor()
    {
        // Arrange
        var loggerMock = Fixture.LoggerMock;

        var configurator = Fixture.Configurator as MiddlewareConfigurator<ApplicationBuilder, IMiddlewareBinding>;

        configurator!.UseMiddleware<MiddlewareA>();
        configurator.UseMiddleware<MiddlewareB>();

        var registerMock = new Mock<Action<MiddlewareBindingDescriptor, HttpContext>>();

        // Mock IApplicationBuilder
        var appBuilderMock = new Mock<IApplicationBuilder>();

        // Set the mock IApplicationBuilder in the configurator
        configurator.GetType()
                    .GetProperty("AppBuilder", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(configurator, appBuilderMock.Object);

        // Act
        configurator.Register(registerMock.Object);

        // Assert
        registerMock.Verify(r => r(It.IsAny<MiddlewareBindingDescriptor>(), It.IsAny<HttpContext>()), Times.Exactly(2));

        var descriptors = configurator.MiddlewareDescriptors ?? new List<MiddlewareBindingDescriptor>();

        foreach (var descriptor in descriptors)
        {
            registerMock.Verify(r => r(descriptor, It.IsAny<HttpContext>()), Times.Once);
        }
    }

    [Fact]
    public void ValidateBindings_ShouldIdentifyAndHandleDuplicates()
    {
        // Arrange
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;
        configurator!.UseMiddleware<TestMiddleware>();
        Configurator.UseMiddleware<TestMiddleware>();

        // Act & Assert
        configurator.Invoking(c => c.ValidateBindings()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OrderMiddlewareDescriptors_ShouldOrderMiddlewaresCorrectly()
    {
    }

    [Fact]
    public void MergeDescriptors_ShouldUpdatePropertiesCorrectly()
    {
    }
}
#endif
