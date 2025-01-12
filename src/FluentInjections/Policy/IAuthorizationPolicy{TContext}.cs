// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using FluentInjections.Context;

namespace FluentInjections.Policies;

/// <summary>
/// Represents a policy that ensures the caller has the necessary permissions to execute an operation. This is crucial for security and access control.
/// </summary>
/// <typeparam name="TContext">The context to apply authorization that is used to authorize the execution of operations.</typeparam>
public interface IAuthorizationPolicy<TContext> : IAuthorizationPolicy where TContext : IContext
{
    /// <summary>
    /// Authorizes the specified action asynchronously.
    /// </summary>
    /// <param name="context">The context that is used to authorize the operation.</param>
    /// <param name="cancellationToken">An optional token that can be used to cancel the operation. Defaults to <see langword="null">.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AuthorizeAsync(TContext context, CancellationToken? cancellationToken = default);
}
