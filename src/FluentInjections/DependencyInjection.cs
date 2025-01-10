// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection;

namespace FluentInjections;

internal static class DependencyInjection
{
    internal static readonly object LockObject = new object();
    internal static IServiceCollection Services { get; set; } = new ServiceCollection();
    public static Assembly[] TargetAssemblies { get; set; }
    internal static IServiceProvider ServiceProvider { get; set; }
    internal static Mock<ILogger> MockLogger { get; set; }
    internal static NetCoreServiceConfigurator ServiceConfigurator { get; set; }
    public static IApplicationBuilder AppBuilder { get; set; }
    internal static NetCoreMiddlewareConfigurator MiddlewareConfigurator { get; set; }
    internal static IModuleRegistry ModuleRegistry
    {
        get
        {
            if (_moduleRegistry is null)
            {
                _moduleRegistry = new ModuleRegistry(Services);
            }

            return _moduleRegistry;
        }
    }

    public static FluentInjectionsNetCoreServiceModule ServiceModule { get; set; }
    public static FluentInjectionsNetCoreMiddlewareModule MiddlewareModule { get; set; }

    private static bool _initializedDependencies;
    private static bool _initiailizedMiddleware;
    private static IModuleRegistry? _moduleRegistry;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static DependencyInjection()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        lock (LockObject)
        {
            ServiceProvider = default!;
            MockLogger = new Mock<ILogger>();
            ServiceConfigurator = new NetCoreServiceConfigurator(Services, LoggerUtility.CreateLogger<NetCoreServiceConfigurator>());
            _initializedDependencies = false;
        }
    }

    /// <summary>
    /// Adds FluentInjections to the service collection by scanning the specified assemblies for <see cref="IModule{TConfigurator}"/> implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for FluentInjections.</param>
    /// <exception cref="InvalidOperationException">Thrown if FluentInjections has already been initialized.</exception>
    internal static void AddFluentInjections(IServiceCollection services, params Assembly[]? assemblies)
    {
        Guard.NotNull(services, nameof(services));

        lock (LockObject)
        {
            if (_initializedDependencies)
            {
                throw new InvalidOperationException("FluentInjections has already been initialized.");
            }

            Services = services;
            TargetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            ServiceModule = new FluentInjectionsNetCoreServiceModule(Services, TargetAssemblies);

            ServiceModule.Load();

            _initializedDependencies = true;
        }
    }

    internal static void UseFluentInjections(IApplicationBuilder builder, params Assembly[]? assemblies)
    {
        lock (LockObject)
        {
            if (!_initializedDependencies)
            {
                throw new InvalidOperationException("FluentInjections has not been initialized. Call AddFluentInjections first.");
            }

            if (_initiailizedMiddleware)
            {
                return;
            }

            AppBuilder = builder;
            MiddlewareConfigurator = new NetCoreMiddlewareConfigurator(
                builder,
                builder.ApplicationServices,
                LoggerUtility.CreateLogger<NetCoreMiddlewareConfigurator>(),
                MiddlewareConfigurator);
            MiddlewareModule = new FluentInjectionsNetCoreMiddlewareModule(builder, assemblies ?? TargetAssemblies);

            MiddlewareModule.Load();

            _initiailizedMiddleware = true;
        }
    }
}
