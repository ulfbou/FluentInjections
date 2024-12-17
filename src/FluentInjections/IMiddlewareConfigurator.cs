using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public interface IMiddlewareConfigurator<TApplication> where TApplication : class
{
    TApplication Application { get; }

    IMiddlewareBinding<TMiddleware, TApplication> UseMiddleware<TMiddleware>() where TMiddleware : class;
    IMiddlewareBinding<TMiddleware, TApplication> RemoveMiddleware<TMiddleware>() where TMiddleware : class;
    IMiddlewareBinding<TMiddleware, TApplication> GetMiddleware<TMiddleware>() where TMiddleware : class;
    void ApplyGroupPolicy<TMiddleware>(string groupName, Action<IMiddlewareBinding<TMiddleware, TApplication>> configure) where TMiddleware : class;
    void ConfigureAll<TMiddleware>(Action<IMiddlewareBinding<TMiddleware, TApplication>> configure) where TMiddleware : class;
}
