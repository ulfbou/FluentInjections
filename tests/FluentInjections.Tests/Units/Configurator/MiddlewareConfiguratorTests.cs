using Autofac;

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
        var loggerMock = new Mock<ILogger<TConfigurator>>();
        var fixture = Fixture;

        var configurator = fixture.Configurator as MiddlewareConfigurator<ContainerBuilder, IMiddlewareBinding>;

        configurator!.UseMiddleware<MiddlewareA>();
        configurator.UseMiddleware<MiddlewareB>();

        var registerMock = new Mock<Action<MiddlewareBindingDescriptor, HttpContext>>();

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
