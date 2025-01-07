// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

/// <summary>
/// Represents a module that provides methods to configure services and middleware within the application using ASP.NET Core.
/// </summary>
internal sealed class FluentInjectionsNetCoreModule : FluentInjectionsModule
{
    private readonly IServiceCollection _services;
    private readonly Microsoft.AspNetCore.Builder.ApplicationBuilder _app;

    public FluentInjectionsNetCoreModule(IServiceCollection services, Microsoft.AspNetCore.Builder.ApplicationBuilder app, Assembly[] assemblies) : base(assemblies)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _app = app ?? throw new ArgumentNullException(nameof(app));
        ValidateBuilder();
    }

    private void ValidateBuilder()
    {
        if (!typeof(IApplicationBuilder).IsAssignableFrom(typeof(ApplicationBuilder)))
        {
            throw new InvalidOperationException($"The builder type must implement {nameof(IApplicationBuilder)}.");
        }
    }

    internal void Load()
    {
        var serviceConfigurator = new NetCoreServiceConfigurator(_services, LoggerUtility.CreateLogger<NetCoreServiceConfigurator>());
        var middlewareConfigurator = new NetCoreMiddlewareConfigurator(_app, LoggerUtility.CreateLogger<NetCoreMiddlewareConfigurator>());

        foreach (var assembly in _assemblies)
        {
            RegisterModulesFromAssembly(assembly, serviceConfigurator, middlewareConfigurator);
        }

        serviceConfigurator.Register();
        middlewareConfigurator.Register();
    }
}