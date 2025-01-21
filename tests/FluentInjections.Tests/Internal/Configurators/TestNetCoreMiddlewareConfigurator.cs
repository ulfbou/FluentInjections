// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

/// <summary>
/// Represents a test middleware configurator that provides methods to configure middleware within the application.
/// </summary>
internal sealed class TestNetCoreMiddlewareConfigurator : NetCoreMiddlewareConfigurator, IMiddlewareConfigurator, ITestMiddlewareConfigurator
{
    public TestNetCoreMiddlewareConfigurator(
        IApplicationBuilder appBuilder,
        ILogger<TestNetCoreMiddlewareConfigurator> logger,
        MiddlewareDescriptor[]? descriptors = null)
        : base(appBuilder, appBuilder.ApplicationServices, logger)
    {
        _descriptors.AddRange(descriptors ?? Array.Empty<MiddlewareDescriptor>());
    }

    /// <inheritdoc />
    public IEnumerable<MiddlewareDescriptor> GetDescriptors() => OrderBindingDescriptors();

    /// <inheritdoc />
    public void RegisterWithAction(Action<MiddlewareDescriptor, HttpContext> registerAction)
    {
        Register(registerAction);
    }
}
