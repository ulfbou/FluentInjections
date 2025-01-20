// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Bindings;
using FluentInjections.Context;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a policy for handling exceptions that occur during the execution of a operation.
/// </summary>
/// <typeparam name="TContext">The type of context that is used to execute the operation.</typeparam>
public interface IExceptionHandlingPolicy<out TContext> : IPolicy<IExceptionHandlingBinding>, IExceptionHandlingPolicy
    where TContext : IContext
{ }