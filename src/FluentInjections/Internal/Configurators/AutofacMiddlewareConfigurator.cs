// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

internal sealed class AutofacMiddlewareConfigurator : MiddlewareConfigurator<ContainerBuilder, IMiddlewareBinding>
{
    public AutofacMiddlewareConfigurator(ContainerBuilder container, ILogger<AutofacMiddlewareConfigurator> logger)
        : base(container, logger)
    { }

    protected override void Register(MiddlewareBindingDescriptor descriptor) => Register(descriptor, null);

    internal override void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext>? register = null)
    {
        if (_dependencyBuilder is not ContainerBuilder builder)
        {
            throw new InvalidOperationException("The provided builder is not supported.");
        }

        var builder = new ContainerBuilder();
        var sp = scope.Resolve<IServiceProvider>();
        builder.RegisterInstance(sp).As<IServiceProvider>().SingleInstance();
        builder.RegisterInstance(sp).As<IServiceScopeFactory>().SingleInstance();

        var container = builder.Build();
        var middlewareInstance = container.Resolve(descriptor.MiddlewareType) as IMiddleware;

        if (middlewareInstance != null)
        {
            scope.Resolve<IApplicationBuilder>().Use(async (context, next) =>
            {
                if (descriptor.IsEnabled && (descriptor.Condition is null || descriptor.Condition.Invoke()))
                {
                    if (register != null)
                    {
                        register(descriptor, context);
                    }
                    else
                    {
                        await middlewareInstance.InvokeAsync(context, next);
                    }
                }
                else
                {
                    await next(context);
                }
            });
        }
    }
}
