using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Utilities;

public static class MiddlewarePipelineHelper
{
    public static RequestDelegate CreatePipeline(IEnumerable<IMiddleware> middlewares, RequestDelegate finalHandler)
    {
        return middlewares.Reverse().Aggregate(finalHandler, (next, middleware) =>
            context => middleware.InvokeAsync(context, next));
    }
}
