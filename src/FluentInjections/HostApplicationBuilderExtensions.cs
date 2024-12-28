// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace FluentInjections;

public static class HostApplicationBuilderExtensions
{
    /// <summary>
    /// Configures FluentInjections as the host application's DI container.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for service and middleware modules.</param>
    /// <returns>The host application builder with FluentInjections configured.</returns>
    public static IHostApplicationBuilder AddFluentInjections(this IHostApplicationBuilder builder, params Assembly[]? assemblies)
    {
        var services = builder.Services;
        DependencyInjection.AddFluentInjections(services, assemblies);
        return builder;
    }
}
