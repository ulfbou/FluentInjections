using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Utilities
{
    public interface IMiddlewarePipelineBuilder
    {
        RequestDelegate Build();
        IMiddlewarePipelineBuilder UseMiddleware(Type middlewareType, params object?[]? args);
        IMiddlewarePipelineBuilder UseMiddleware<TMiddleware>(params object?[]? args) where TMiddleware : class, IMiddleware;
    }
}