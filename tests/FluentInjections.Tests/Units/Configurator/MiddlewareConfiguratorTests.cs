using FluentAssertions;
using FluentAssertions.Common;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection;
using System.Reflection.PortableExecutable;

namespace FluentInjections.Tests.Units.Configurator;

/// <summary>
/// Represents a class for testing <see cref="MiddlewareConfigurator{TService, TDescriptor}"/> and <see cref="IMiddlewareConfigurator"/>.
/// </summary>
internal abstract partial class MiddlewareConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    : ConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    where TConfigurator : class, IMiddlewareConfigurator, ITestMiddlewareConfigurator
    where TServices : class, IServiceCollection
    where TProvider : class, IServiceProvider
    where TFixture : class, IMiddlewareConfiguratorFixture, IConfiguratorFixture<TConfigurator, TServices, TProvider>, new()
{
    public void Constructor_ShouldInitializeWithLogger()
    {
        // Arrange
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;

        // Act & Assert
        configurator?.Logger.Should().NotBeNull();
    }

    public void UseMiddleware_ShouldAddMiddlewareDescriptor()
    {
        // Arrange
        var binding = Configurator.UseMiddleware<TestMiddleware>();
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;

        // Act & Assert
        configurator?.Descriptors.Should().Contain(binding.Descriptor);
    }

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

    public void ValidateBindings_ShouldIdentifyAndHandleDuplicates()
    {
        // Arrange
        var configurator = Configurator as MiddlewareConfigurator<TServices, IMiddlewareBinding>;
        configurator!.UseMiddleware<TestMiddleware>();
        Configurator.UseMiddleware<TestMiddleware>();

        // Act & Assert
        configurator.Invoking(c => c.ValidateBindings()).Should().Throw<InvalidOperationException>();
    }

    public void OrderMiddlewareDescriptors_ShouldOrderMiddlewaresCorrectly()
    {
        // Arrange
        Configurator.UseMiddleware<MiddlewareA>()
                    .WithPriority(3);
        Configurator.UseMiddleware<MiddlewareB>()
                    .WithPriority(1);
        Configurator.UseMiddleware<MiddlewareC>()
                    .WithPriority(2);

        // Act
        Configurator.Register();
        var orderedDescriptors = Configurator.GetDescriptors();

        // Assert
        orderedDescriptors.Should()
                          .HaveCount(3);
        orderedDescriptors.Should()
                          .BeInAscendingOrder(d => d.Priority);
    }

    public void WarnAndReplace_DuplicateRegistrations_Should_LogWarningAndReplace()
    {
        // Arrange
        ConfigureMiddlewareDescriptors(ConflictResolutionMode.WarnAndReplace);

        // Act
        Configurator.Register();
        var descriptors = Configurator.GetDescriptors().Where(d => d.MiddlewareType == typeof(MiddlewareA));

        // Assert
        descriptors.Should().HaveCount(1);
    }

    public void Merge_DuplicateRegistrations_Should_Merge()
    {
        // Arrange
        ConfigureMiddlewareDescriptors(ConflictResolutionMode.Merge);

        // Act
        Configurator.Register();
        var descriptors = Configurator.GetDescriptors().Where(d => d.MiddlewareType == typeof(MiddlewareA));

        // Assert
        descriptors.Should().HaveCount(1);
        var descriptor = descriptors.First();
        descriptor.Priority.Should().Be(3);
    }

    public void Replace_DuplicateRegistrations_Should_Replace()
    {
        // Arrange
        ConfigureMiddlewareDescriptors(ConflictResolutionMode.Replace);

        // Act
        Configurator.Register();
        var descriptors = Configurator.GetDescriptors().Where(d => d.MiddlewareType == typeof(MiddlewareA));

        // Assert
        descriptors.Should().HaveCount(1);
    }

    public void Prevent_DuplicateRegistrations_Should_ThrowInvalidOperationException()
    {
        // Arrange
        ConfigureMiddlewareDescriptors(ConflictResolutionMode.Prevent);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => Configurator.Register());
    }

    public void Ignore_DuplicateRegistrations_Should_Ignore()
    {
        // Arrange
        ConfigureMiddlewareDescriptors(ConflictResolutionMode.Ignore);

        // Act
        Configurator.Register();
        var descriptors = Configurator.GetDescriptors().Where(d => d.MiddlewareType == typeof(MiddlewareA));

        // Assert
        descriptors.Should().HaveCount(2);
    }

    public void ConfigureMiddlewareDescriptors()
    {
        Configurator.UseMiddleware<MiddlewareA>()
                    .WithPriority(3)
                    .WithName("DescriptorA");
        Configurator.UseMiddleware<MiddlewareB>()
                    .WithPriority(1)
                    .WithName("DescriptorB");
        Configurator.UseMiddleware<MiddlewareC>()
                    .WithPriority(2)
                    .WithName("DescriptorC");
    }

    public void ConfigureMiddlewareDescriptors(ConflictResolutionMode mode)
    {
        Configurator.ConflictResolution = mode;
        Configurator.UseMiddleware<MiddlewareA>()
                    .WithPriority(3);
        Configurator.UseMiddleware<MiddlewareA>()
                    .WithPriority(1);
        Configurator.UseMiddleware<MiddlewareB>()
                    .WithPriority(2);
    }
}
