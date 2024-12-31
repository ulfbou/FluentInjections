// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Microsoft.AspNetCore.Builder;
using FluentInjections.Internal.Descriptors;
using Microsoft.AspNetCore.Http;

namespace FluentInjections.Internal.Configurators;

public sealed class AutofacMiddlewareConfigurator : MiddlewareConfigurator<IApplicationBuilder>
{
    private readonly ContainerBuilder _builder;

    public AutofacMiddlewareConfigurator(ContainerBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    protected override void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext, IApplicationBuilder>? register = null)
    {
        _builder.RegisterType(descriptor.MiddlewareType).As<IMiddleware>().InstancePerDependency();

        _builder.RegisterBuildCallback(container =>
        {
            var app = container.Resolve<IApplicationBuilder>();

            app.Use(async (HttpContext context, RequestDelegate next) =>
            {
                if (descriptor.IsEnabled && (descriptor.Condition == null || descriptor.Condition.Invoke()))
                {
                    if (register != null)
                    {
                        register(descriptor, context, app);
                    }
                    else
                    {
                        var middlewareInstance = app.ApplicationServices.GetService(descriptor.MiddlewareType) as IMiddleware;
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
        });
    }
}
