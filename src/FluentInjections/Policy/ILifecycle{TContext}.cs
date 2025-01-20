// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Context;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a lifecycle that is executed before and after an operation, as well as on success and failure. 
/// </summary>
/// <typeparam name="TContext">The type of the context.</typeparam>
public interface ILifecycle<TContext> where TContext : class
{
    Task OnStartAsync(TContext context, CancellationToken? cancellationToken = default);
    Task OnEndAsync(TContext context, CancellationToken? cancellationToken = default);
    Task OnErrorAsync(Exception exception, TContext context, Func<Task> handleError, CancellationToken? cancellationToken = default);
    Task OnSuccessAsync(TContext context, CancellationToken? cancellationToken = default);
}
