// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Context;

using System.Threading;

namespace FluentInjections.Policies;

/// <summary>
/// Represents a binding that provides methods to apply to contexts. 
/// </summary>
/// <typeparam name="TBinding">The type of binding that the policy is associated with.</typeparam>
public interface IPolicy<TBinding> : IPolicy where TBinding : IBinding
{
    /// <summary>
    /// Applies the policy to the context.
    /// </summary>
    /// <typeparam name="TContext">The type of context to apply the policy to.</typeparam>
    /// <param name="context">The context to apply the policy to.</param>
    void Apply<TContext>(TContext context);

    /// <summary>
    /// Applies the policy to the context asynchronously.
    /// </summary>
    /// <typeparam name="TContext">The type of context to apply the policy to.</typeparam>
    /// <param name="context">The context to apply the policy to.</param>
    /// <param name="cancellationToken">The cancellation token that is used to cancel the operation.</param>
    Task ApplyAsync<T>(T context, CancellationToken? cancellationToken = default);
}
