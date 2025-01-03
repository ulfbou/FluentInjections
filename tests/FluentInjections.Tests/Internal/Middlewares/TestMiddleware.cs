﻿// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

using System.Diagnostics;

namespace FluentInjections.Tests.Internal.Middlewares;

internal class TestMiddlewareBase
{
    protected readonly List<Type> _pipelineOrder;

    public TestMiddlewareBase(List<Type> pipelineOrder)
    {
        _pipelineOrder = pipelineOrder;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Debug.WriteLine($"Invoking {GetType().Name}");
        _pipelineOrder.Add(GetType());
        await next(context);
    }
}

internal class TestMiddleware
{
    public static int id = 0;
    public static List<int> CallOrder { get; } = new();

    public int Id { get; set; }

    public TestMiddleware() => Id = ++id;

    public virtual async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        CallOrder.Add(Id);
        await next(context);
    }
}

internal class MiddlewareA(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareB(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareC(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareD(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareE(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareF(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }

internal class TestMiddleware<TOptions> : TestMiddleware where TOptions : class
{
    public static TOptions LastOptions = default!;
    private readonly TOptions _options;
    private readonly RequestDelegate _next;

    public TestMiddleware(RequestDelegate next, TOptions options) : base()
    {
        _next = next;
        _options = options;
        LastOptions = options;
    }

    public override async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        CallOrder.Add(1);
        LastOptions = _options;
        await _next(context);
    }
}

internal sealed class TestOptions
{
    public required string Value { get; set; }
}
