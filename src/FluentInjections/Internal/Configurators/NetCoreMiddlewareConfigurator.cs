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

internal sealed class NetCoreMiddlewareConfigurator<TDependencyBuilder> : MiddlewareConfigurator<TDependencyBuilder, IMiddlewareBinding>
    where TDependencyBuilder : class
{
    internal NetCoreMiddlewareConfigurator(TDependencyBuilder builder, ILogger<NetCoreMiddlewareConfigurator<TDependencyBuilder>> logger)
        : base(builder, logger)
    { }

    protected override void Register(MiddlewareBindingDescriptor descriptor) => Register(descriptor, null);

    internal override void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext>? register = null)
    {
        if (_dependencyBuilder is not IApplicationBuilder builder)
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
                    register(descriptor, context);
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
