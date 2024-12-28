// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Internal.Registries;
using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Reflection;

namespace FluentInjections;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures FluentInjections as the DI container for the application.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for service and middleware modules.</param>
    /// <returns>The application builder with FluentInjections configured.</returns>
    public static IApplicationBuilder AddFluentInjections(this IApplicationBuilder builder, params Assembly[]? assemblies)
    {
        var services = builder.ApplicationServices.GetRequiredService<IServiceCollection>();
        DependencyInjection.AddFluentInjections(services, assemblies);
        return builder;
    }

    /// <summary>
    /// Configures FluentInjections as middleware for the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="assemblies">Optional. The assemblies to scan for middleware modules.</param>
    /// <returns>The application builder with FluentInjections configured.</returns>
    public static IApplicationBuilder UseFluentInjections(this IApplicationBuilder builder, params Assembly[]? assemblies)
    {
        DependencyInjection.UseFluentInjections(builder, assemblies);
        return builder;
    }
}
