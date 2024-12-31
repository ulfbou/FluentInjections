// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Helpers;

public static class PipelineHelper
{
    internal static RequestDelegate BuildPipeline(MiddlewareConfigurator<IApplicationBuilder> configurator, IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);
        configurator.Register((descriptor, context, appBuilder) =>
        {
            appBuilder.Use(async (ctx, next) =>
            {
                var middleware = (IMiddleware)ActivatorUtilities.CreateInstance(appBuilder.ApplicationServices, descriptor.MiddlewareType);
                await middleware.InvokeAsync(ctx, next);
            });
        });
        return app.Build();
    }
}
