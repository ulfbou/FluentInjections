// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
