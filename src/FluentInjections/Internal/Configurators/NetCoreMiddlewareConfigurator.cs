// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

internal sealed class NetCoreMiddlewareConfigurator<TBuilder> : MiddlewareConfigurator<TBuilder>
{
    private readonly TBuilder _app;

    internal NetCoreMiddlewareConfigurator(TBuilder app, ILogger<NetCoreMiddlewareConfigurator<TBuilder>> logger)
        : base(logger)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
    }

    protected override void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext, TBuilder>? register = null)
    {
        if (_app is not IApplicationBuilder builder)
        {
            throw new InvalidOperationException("The provided builder is not supported.");
        }

        var sp = builder.ApplicationServices;
        builder.Use(async (context, next) =>
        {
            if (descriptor.IsEnabled && (descriptor.Condition == null || descriptor.Condition.Invoke()))
            {
                if (register != null)
                {
                    register(descriptor, context, _app);
                }
                else
                {
                    var middlewareInstance = sp.GetService(descriptor.MiddlewareType) as IMiddleware;
                    if (middlewareInstance != null)
                    {
                        await middlewareInstance.InvokeAsync(context, next);
                    }
                    else
                    {
                        await next(context);
                    }
                }
            }
            else
            {
                await next(context);
            }
        });
    }
}
