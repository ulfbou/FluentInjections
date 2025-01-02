using FluentAssertions;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Units.Configurator;

public abstract partial class MiddlewareConfiguratorTests<TConfigurator, TContainer, TFixture> : ConfiguratorTests<TConfigurator, TContainer, TFixture>
    where TConfigurator : class, IMiddlewareConfigurator
    where TContainer : class
    where TFixture : class, IMiddlewareConfiguratorFixture<TConfigurator, TContainer>, new()
{
    [Fact]
    public void Constructor_ShouldInitializeWithLogger()
    {
        // Arrange
        var configurator = Configurator as MiddlewareConfigurator<TContainer, IMiddlewareBinding>;

        // Act & Assert
        configurator?.Logger.Should().NotBeNull();
    }

    [Fact]
    public void UseMiddleware_ShouldAddMiddlewareDescriptor()
    {
        // Arrange
        var binding = Configurator.UseMiddleware<TestMiddleware>();
        var configurator = Configurator as MiddlewareConfigurator<TContainer, IMiddlewareBinding>;

        // Act & Assert
        configurator?.Descriptors.Should().Contain(binding.Descriptor);
    }

    [Fact]
    public void RemoveMiddleware_ShouldRemoveMiddlewareDescriptor()
    {
        // Arrange
        var binding = Configurator.UseMiddleware<TestMiddleware>();
        var configurator = Configurator as MiddlewareConfigurator<TContainer, IMiddlewareBinding>;

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
        var configurator = Configurator as MiddlewareConfigurator<TContainer, IMiddlewareBinding>;

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
        Mock<ILogger<TConfigurator>> loggerMock = new Mock<ILogger<TConfigurator>>();
        var fixture = new TFixture();
        Configurator.UseMiddleware<MiddlewareA>();
        Configurator.UseMiddleware<MiddlewareB>();

        // Act
        Configurator.Register();

        // Assert
        // Verify that the Register method was called for each descriptor
        // This can be done with mocks or by checking internal state changes
        // For example, using a mock logger to verify method calls
        // loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeast(2));
    }

    [Fact]
    public void ValidateBindings_ShouldIdentifyAndHandleDuplicates()
    {
        // Arrange
        var configurator = Configurator as MiddlewareConfigurator<TContainer, IMiddlewareBinding>;
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
