// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Bindings;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a policy for handling exceptions that occur during the execution of a operation.
/// </summary>
public interface IExceptionHandlingPolicy : IPolicy<IExceptionHandlingBinding> { }
