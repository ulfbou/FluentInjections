// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Bindings;
using FluentInjections.Context;

namespace FluentInjections.Policies;

/// <summary>
/// Represents a policy that validates the execution of an operation.
/// </summary>
/// <typeparam name="TContext">The type of the context.</typeparam>
public interface IValidationPolicy<TContext> : IPolicy<IValidationBinding> where TContext : IContext
{
    /// <summary>
    /// Validates the execution of an operation asynchronously.
    /// </summary>
    /// <param name="context">The context that is used to validate the operation.</param>
    /// <param name="cancellationToken">An optional token that can be used to cancel the operation. Defaults to <see langword="null">.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ValidateAsync(TContext context, CancellationToken? cancellationToken = default);
}
