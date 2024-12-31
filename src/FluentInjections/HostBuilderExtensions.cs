// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace FluentInjections;

public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures FluentInjections as the host's DI container.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for service and middleware modules.</param>
    /// <returns>The host builder with FluentInjections configured.</returns>
    public static IHostBuilder AddFluentInjections(this IHostBuilder builder, params Assembly[]? assemblies)
    {
        builder.ConfigureServices((context, services) =>
        {
            DependencyInjection.AddFluentInjections(services, builder, assemblies);
        });

        return builder;
    }
}
