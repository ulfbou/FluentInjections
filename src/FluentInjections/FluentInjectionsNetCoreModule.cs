// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Internal.Configurators;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

/// <summary>
/// Represents a module that provides methods to configure services and middleware within the application using ASP.NET Core.
/// </summary>
internal sealed class FluentInjectionsNetCoreModule<TBuilder> : FluentInjectionsModule
{
    private readonly IServiceCollection _services;
    private readonly TBuilder _app;

    public FluentInjectionsNetCoreModule(IServiceCollection services, TBuilder app, Assembly[] assemblies) : base(assemblies)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _app = app ?? throw new ArgumentNullException(nameof(app));
        ValidateBuilder();
    }

    private void ValidateBuilder()
    {
        if (!typeof(IApplicationBuilder).IsAssignableFrom(typeof(TBuilder)))
        {
            throw new InvalidOperationException($"The builder type must implement {nameof(IApplicationBuilder)}.");
        }
    }

    protected override void Load(ContainerBuilder builder) => throw new NotImplementedException();

    internal void Load()
    {
        var serviceConfigurator = new NetCoreServiceConfigurator(_services);
        var middlewareConfigurator = new NetCoreMiddlewareConfigurator<TBuilder>(_app);

        foreach (var assembly in _assemblies)
        {
            RegisterModulesFromAssembly(assembly, serviceConfigurator, middlewareConfigurator);
        }

        serviceConfigurator.Register();
        middlewareConfigurator.Register();
    }
}