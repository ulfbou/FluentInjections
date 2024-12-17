using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public interface IMiddlewareBinding<TMiddleware, TApplication> where TMiddleware : class where TApplication : class
{
    IMiddlewareBinding<TMiddleware, TApplication> WithPriority(int priority);
    IMiddlewareBinding<TMiddleware, TApplication> WithPriority(Func<int> priority);
    IMiddlewareBinding<TMiddleware, TApplication> WithPriority<T>(Func<T, int> priority);
    IMiddlewareBinding<TMiddleware, TApplication> WithExecutionPolicy<T>(Action<T> value) where T : class;
    IMiddlewareBinding<TMiddleware, TApplication> WithMetadata<TMetadata>(TMetadata metadata);
    IMiddlewareBinding<TMiddleware, TApplication> WithFallback(Func<TMiddleware, Task> fallback);
    IMiddlewareBinding<TMiddleware, TApplication> WithOptions<TOptions>(TOptions options) where TOptions : class;
    IMiddlewareBinding<TMiddleware, TApplication> WithTag(string tag);
    IMiddlewareBinding<TMiddleware, TApplication> When(Func<bool> func);
    IMiddlewareBinding<TMiddleware, TApplication> When<TContext>(Func<TContext, bool> func);
    IMiddlewareBinding<TMiddleware, TApplication> InGroup(string group);
    IMiddlewareBinding<TMiddleware, TApplication> DependsOn<TOtherMiddleware>();
    IMiddlewareBinding<TMiddleware, TApplication> Precedes<TPrecedingMiddleware>();
    IMiddlewareBinding<TMiddleware, TApplication> Follows<TFollowingMiddleware>();
    IMiddlewareBinding<TMiddleware, TApplication> Disable();
    IMiddlewareBinding<TMiddleware, TApplication> Enable();
    IMiddlewareBinding<TMiddleware, TApplication> RequireEnvironment(string environment);
    IMiddlewareBinding<TMiddleware, TApplication> WithTimeout(TimeSpan timeout);
    IMiddlewareBinding<TMiddleware, TApplication> OnError(Func<Exception, Task> errorHandler);
}
