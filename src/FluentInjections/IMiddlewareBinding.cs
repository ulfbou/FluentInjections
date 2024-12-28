// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Represents a middleware binding that provides methods to bind and configure middleware components within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define middleware bindings.
/// </remarks>
public interface IMiddlewareBinding : IBinding
{
    MiddlewareBindingDescriptor Descriptor { get; }
}
