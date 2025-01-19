// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Internal.Configurators;

/// <summary>
/// A marker interface that represents a test middleware configurator that provides methods to configure middleware within the application.
/// </summary>
public interface ITestMiddlewareConfigurator : IMiddlewareConfigurator, ITestConfigurator<MiddlewareDescriptor, IMiddlewareBinding>
{
    /// <summary>
    /// Registers a middleware binding with the application builder.
    /// </summary>
    /// <param name="registerAction">The action to register the middleware binding with the application builder.</param>
    /// <remarks>
    /// This method is used to register a middleware binding with the application builder.
    /// </remarks>
    void RegisterWithAction(Action<MiddlewareDescriptor, HttpContext> registerAction);
}
