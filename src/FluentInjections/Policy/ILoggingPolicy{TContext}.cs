// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Policy;

/// <summary>
/// Represents a policy that logs the execution of operations for monitoring, debugging, or auditing purposes. 
/// </summary>
/// <typeparam name="TContext">The type of the context that is used to log the execution of operations.</typeparam>
public interface ILoggingPolicy<TContext> : ILoggingPolicy
{
    /// <summary>
    /// Logs the execution of an operation asynchronously.
    /// </summary>
    /// <param name="context">The context that is used to log the operation.</param>
    /// <param name="message">The message that is logged.</param>
    /// <param name="exception">An optional exception that is logged. Defaults to <see langword="null">.</param>
    /// <param name="cancellationToken">An optional token that can be used to cancel the operation. Defaults to <see langword="null">.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LogAsync(TContext context, string message, Exception? exception = default, CancellationToken? cancellationToken = default);
    void LogAsync(TContext context, CancellationToken? cancellationToken = default);
}
