// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection;

namespace FluentInjections;

internal static class DependencyInjection
{
    internal static readonly object LockObject = new object();
    internal static IServiceCollection Services { get; private set; } = new ServiceCollection();
    internal static IServiceProvider ServiceProvider { get; private set; }
    internal static Mock<ILogger> MockLogger { get; private set; }
    internal static NetCoreServiceConfigurator ServiceConfigurator { get; private set; }
    internal static NetCoreMiddlewareConfigurator MiddlewareConfigurator { get; private set; }
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

    public static FluentInjectionsNetCoreModule Module { get; private set; }

    private static bool _initializedDependencies;
    private static bool _initiailizedMiddleware;
    private static IModuleRegistry? _moduleRegistry;

    static DependencyInjection()
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
            var TargetAssemblies = assemblies?.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            Module = new FluentInjectionsNetCoreModule(Services, TargetAssemblies);

            Module.Load();

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

            MiddlewareConfigurator = new NetCoreMiddlewareConfigurator(
                builder,
                builder.ApplicationServices,
                LoggerUtility.CreateLogger<NetCoreMiddlewareConfigurator>(),
                MiddlewareConfigurator);

            _initiailizedMiddleware = true;
        }
    }
}
