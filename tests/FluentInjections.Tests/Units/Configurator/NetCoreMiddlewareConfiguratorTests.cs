// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Units.Configurator;

public class NetCoreMiddlewareConfiguratorTests
{
    private readonly InternalNetCoreMiddlewareConfiguratorTests _internal = new InternalNetCoreMiddlewareConfiguratorTests();

    public NetCoreMiddlewareConfiguratorTests()
    {
        _internal.Configurator = new NetCoreMiddlewareConfigurator<ApplicationBuilder>(_internal.Fixture.AppBuilder, _internal.Fixture.LoggerMock.Object);
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
    public void Register_ShouldInvokeRegisterMethodForEachDescriptor()
    {
        // Arrange
        var configurator = _internal.Configurator;

        // Act
        configurator.UseMiddleware<MiddlewareA>().InGroup("TestGroup");
        configurator.UseMiddleware<MiddlewareB>().InGroup("TestGroup");
        configurator.Register();
        _internal.BuildProvider();
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
    public void MergeDescriptors_ShouldUpdatePropertiesCorrectly()
    {
        _internal.MergeDescriptors_ShouldUpdatePropertiesCorrectly();
    }
}
