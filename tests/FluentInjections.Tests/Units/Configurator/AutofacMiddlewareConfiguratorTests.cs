// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Utility.Fixtures;

namespace FluentInjections.Tests.Units.Configurator;

public class AutofacMiddlewareConfiguratorTests
{
    private readonly AutofacMiddlewareConfigurator _configurator;
    private readonly InternalAutofacMiddlewareConfiguratorTests _internal;

    public AutofacMiddlewareConfiguratorTests()
    {
        _internal = new InternalAutofacMiddlewareConfiguratorTests();

        var fixture = _internal.Fixture;
        _configurator = _internal.Configurator;
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


    }

    [Fact]
    public void Register_ShouldInvokeRegisterMethodForEachDescriptor()
    {
        _internal.Register_ShouldInvokeRegisterMethodForEachDescriptor();
    }

    [Fact]
    public void ValidateBindings_ShouldIdentifyAndHandleDuplicates()
    {
        // Arrange
        _configurator.UseMiddleware<MiddlewareA>();
        _configurator.UseMiddleware<MiddlewareB>();

        // Act & Assert
        _configurator.Invoking(c => c.ValidateBindings()).Should().NotThrow();
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
