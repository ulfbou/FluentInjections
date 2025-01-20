using FluentInjections.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a caching policy that can be executed with a type safe context.
/// </summary>
/// <typeparam name="TContext">The type of context that is used to execute the policy.</typeparam>
public interface ICachingPolicy<TContext> : ICachingPolicy where TContext : IContext
{
    /// <summary>
    /// Gets the cached result asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the result to get.</typeparam>
    /// <param name="action">The action to get the result.</param>
    /// <param name="context">The context to get the result with.</param>
    /// <param name="cancellationToken">An optional token that can be used to cancel the operation. Defaults to <see langword="null">.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result.</returns>
    Task<T> GetCachedResultAsync<T>(Func<TContext, Task<T>> action, TContext context, CancellationToken? cancellationToken = default);
}
