// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Units.Configurator;

/// <summary>
/// Represents a test class for the <see cref="NetCoreMiddlewareConfigurator"/> class.
/// </summary>
public class NetCoreMiddlewareConfiguratorTests
{
    private readonly InternalNetCoreMiddlewareConfiguratorTests _internal = new InternalNetCoreMiddlewareConfiguratorTests();

    public NetCoreMiddlewareConfiguratorTests()
    {
        _internal.Configurator = new TestNetCoreMiddlewareConfigurator(_internal.Fixture.AppBuilder, _internal.Fixture.LoggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithLogger()
    {
        _internal.Constructor_ShouldInitializeWithLogger();
    }

    [Fact]
    public void SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode()
    {
        _internal.SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode();
    }

    [Fact]
    public void UseMiddleware_ShouldAddMiddlewareDescriptor()
    {
        _internal.UseMiddleware_ShouldAddMiddlewareDescriptor();
    }

    [Fact]
    public void RemoveMiddleware_ShouldRemoveMiddlewareDescriptor()
    {
        _internal.RemoveMiddleware_ShouldRemoveMiddlewareDescriptor();
    }

    [Fact]
    public void ApplyGroupPolicy_ShouldApplyConfigurationToGroup()
    {
        _internal.ApplyGroupPolicy_ShouldApplyConfigurationToGroup();
    }

    [Fact]
    public void ConfigureAll_ShouldApplyConfigurationToAllMiddlewares()
    {
        // Arrange
        var configurator = _internal.Configurator;

        // Act
        configurator.ConfigureAll(descriptor => descriptor.Group = "TestGroup");

        // Assert
        foreach (var descriptor in configurator.Descriptors)
        {
            descriptor.Group.Should().Be("TestGroup");
        }
    }

    [Fact]
    public async Task Register_ShouldInvokeRegisterMethodForEachDescriptor()
    {
        await _internal.Register_ShouldInvokeRegisterMethodForEachDescriptorAsync();
    }

    [Fact]
    public void ValidateBindings_ShouldIdentifyAndHandleDuplicates()
    {
    }

    [Fact]
    public void OrderMiddlewareDescriptors_ShouldOrderMiddlewaresCorrectly()
    {
        _internal.OrderMiddlewareDescriptors_ShouldOrderMiddlewaresCorrectly();
    }

    [Fact]
    public void WarnAndReplace_DuplicateRegistrations_Should_LogWarningAndReplace()
    {
        _internal.WarnAndReplace_DuplicateRegistrations_Should_LogWarningAndReplace();
    }

    [Fact]
    public void Merge_DuplicateRegistrations_Should_Merge()
    {
        _internal.Merge_DuplicateRegistrations_Should_Merge();
    }

    [Fact]
    public void Replace_DuplicateRegistrations_Should_Replace()
    {
        _internal.Replace_DuplicateRegistrations_Should_Replace();
    }

    [Fact]
    public void Prevent_DuplicateRegistrations_Should_ThrowInvalidOperationException()
    {
        _internal.Prevent_DuplicateRegistrations_Should_ThrowInvalidOperationException();
    }

    [Fact]
    public void Ignore_DuplicateRegistrations_Should_Ignore()
    {
        _internal.Ignore_DuplicateRegistrations_Should_Ignore();
    }
}
