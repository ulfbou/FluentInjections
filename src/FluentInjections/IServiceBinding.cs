// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections;

/// <summary>
/// Represents a service binding that provides methods to bind and configure services within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define service bindings.
/// </remarks>
public interface IServiceBinding : IBinding
{
    ServiceBindingDescriptor Descriptor { get; }
}
