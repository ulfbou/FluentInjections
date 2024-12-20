using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Provides functionality to configure middleware binding for the application.
/// </summary>
public interface IMiddlewareBinding
{
    /// <summary>
    /// Sets the instance of the middleware.
    /// </summary>
    IMiddlewareBinding WithInstance(object instance);

    /// <summary>
    /// Sets the priority of the middleware.
    /// </summary>
    /// <param name="priority">The priority value.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithPriority(int priority);

    /// <summary>
    /// Sets the priority of the middleware using a function.
    /// </summary>
    /// <param name="priority">The function to determine the priority.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithPriority(Func<int> priority);

    /// <summary>
    /// Sets the priority of the middleware using a function that takes a context parameter.
    /// </summary>
    /// <typeparam name="TContext">The type of the context parameter.</typeparam>
    /// <param name="priority">The function to determine the priority.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithPriority<TContext>(Func<TContext, int> priority);

    /// <summary>
    /// Sets the execution policy for the middleware.
    /// </summary>
    /// <typeparam name="TPolicy">The type of the policy.</typeparam>
    /// <param name="value">The action to configure the policy.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithExecutionPolicy<TPolicy>(Action<TPolicy> value) where TPolicy : class;

    /// <summary>
    /// Attaches metadata to the middleware.
    /// </summary>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    /// <param name="metadata">The metadata to attach.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithMetadata<TMetadata>(TMetadata metadata);

    /// <summary>
    /// Sets a fallback function for the middleware.
    /// </summary>
    /// <param name="fallback">The fallback function.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithFallback(Func<object, Task> fallback);

    /// <summary>
    /// Sets options for the middleware.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    /// <param name="options">The options to set.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithOptions<TOptions>(TOptions options) where TOptions : class;

    /// <summary>
    /// Tags the middleware with a specified tag.
    /// </summary>
    /// <param name="tag">The tag to assign.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithTag(string tag);

    /// <summary>
    /// Sets a condition under which the middleware should be executed.
    /// </summary>
    /// <param name="func">The function to determine the condition.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding When(Func<bool> func);

    /// <summary>
    /// Sets a condition under which the middleware should be executed using a context parameter.
    /// </summary>
    /// <typeparam name="TContext">The type of the context parameter.</typeparam>
    /// <param name="func">The function to determine the condition.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding When<TContext>(Func<TContext, bool> func);

    /// <summary>
    /// Groups the middleware into a specified group.
    /// </summary>
    /// <param name="group">The name of the group.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding InGroup(string group);

    /// <summary>
    /// Sets a dependency on another middleware.
    /// </summary>
    /// <typeparam name="TOtherMiddleware">The type of the other middleware.</typeparam>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding DependsOn<TOtherMiddleware>();

    /// <summary>
    /// Sets the middleware to precede another middleware.
    /// </summary>
    /// <typeparam name="TPrecedingMiddleware">The type of the preceding middleware.</typeparam>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding Precedes<TPrecedingMiddleware>();

    /// <summary>
    /// Sets the middleware to follow another middleware.
    /// </summary>
    /// <typeparam name="TFollowingMiddleware">The type of the following middleware.</typeparam>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding Follows<TFollowingMiddleware>();

    /// <summary>
    /// Disables the middleware.
    /// </summary>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding Disable();

    /// <summary>
    /// Enables the middleware.
    /// </summary>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding Enable();

    /// <summary>
    /// Sets the required environment for the middleware.
    /// </summary>
    /// <param name="environment">The required environment.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding RequireEnvironment(string environment);

    /// <summary>
    /// Sets a timeout for the middleware execution.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding WithTimeout(TimeSpan timeout);

    /// <summary>
    /// Sets an error handler for the middleware.
    /// </summary>
    /// <param name="errorHandler">The function to handle errors.</param>
    /// <returns>The middleware binding instance.</returns>
    IMiddlewareBinding OnError(Func<Exception, Task> errorHandler);
}
