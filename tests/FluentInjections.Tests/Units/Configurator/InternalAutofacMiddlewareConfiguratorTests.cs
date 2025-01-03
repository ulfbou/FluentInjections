// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Configurators;
using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Internal.Utils;
using Moq;
using Microsoft.Extensions.Logging;
using FluentInjections.Tests.Internal.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace FluentInjections.Tests.Units.Configurator;

internal sealed class InternalAutofacMiddlewareConfiguratorTests
    : MiddlewareConfiguratorTests<AutofacMiddlewareConfigurator, ContainerBuilder, AutofacMiddlewareConfiguratorFixture>
{
    private IContainer? _container;
    private IComponentContext? _context;

    public InternalAutofacMiddlewareConfiguratorTests() : base()
    {
        DependencyBuilder = new ContainerBuilder();

        DependencyBuilder.RegisterInstance(LoggerUtility.CreateLogger<AutofacMiddlewareConfigurator>())
            .AsImplementedInterfaces()
            .SingleInstance();
        DependencyBuilder.RegisterInstance(Mock.Of<ILoggerFactory>())
            .AsImplementedInterfaces()
            .SingleInstance();

        DependencyBuilder.RegisterType<TestMiddleware>()
            .AsSelf()
            .InstancePerDependency();

        DependencyBuilder.RegisterType<TestMiddleware>()
            .Named<TestMiddleware>("TestMiddleware")
            .InstancePerDependency();

        DependencyBuilder.RegisterType<AutofacMiddlewareConfigurator>()
            .AsImplementedInterfaces()
            .InstancePerDependency();

        var appBuilder = new ApplicationBuilder(Mock.Of<IServiceProvider>());
        DependencyBuilder.RegisterInstance(appBuilder)
            .AsImplementedInterfaces()
            .SingleInstance();
    }

    internal override void BuildProvider()
    {
        if (_container is not null)
        {
            throw new InvalidOperationException("ServiceProvider is already built. Ensure that BuildProvider is called only once.");
        }

        _context = _container!.BeginLifetimeScope();
    }

    protected override T? GetService<T>() where T : class
    {
        if (_context is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetService<T>.");
        }
        return _context.ResolveOptional<T>();
    }

    protected override T GetRequiredService<T>()
    {
        if (_context is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredService<T>.");
        }
        return _context.Resolve<T>();
    }

    protected override object? GetRequiredNamedService<T>(string name)
    {
        if (_context is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredNamedService<T>.");
        }
        return _context.GetNamedRequiredService<T>(name);
    }
}
