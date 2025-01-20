// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.



// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using FluentInjections.Context;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a policy that transforms the execution of an operation.
/// </summary>
/// <typeparam name="TContext">The type of the context that is used to transform the execution of an operation.</typeparam>
public interface ITransformationPolicy<TContext> : IContext
{
    /// <summary>
    /// Transforms the execution of an operation asynchronously.
    /// </summary>
    /// <param name="context">The context that is used to transform the operation.</param>
    /// <param name="cancellationToken">An optional token that can be used to cancel the operation. Defaults to <see langword="null">.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task TransformAsync(TContext context, CancellationToken? cancellationToken = default);
}
