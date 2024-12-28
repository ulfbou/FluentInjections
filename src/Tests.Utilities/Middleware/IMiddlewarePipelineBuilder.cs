// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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