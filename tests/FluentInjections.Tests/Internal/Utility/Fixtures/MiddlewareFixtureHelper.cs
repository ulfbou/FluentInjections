// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Internal.Middlewares;

using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal class MiddlewareFixtureHelper<TBuilder, TConfigurator>
    where TBuilder : class
    where TConfigurator : class, IServiceConfigurator
{
    public TBuilder DependencyBuilder { get; }
    public TConfigurator ServiceConfigurator { get; }

    public MiddlewareFixtureHelper(TBuilder dependencyBuilder, TConfigurator serviceConfigurator)
    {
        DependencyBuilder = dependencyBuilder;
        ServiceConfigurator = serviceConfigurator;

        // Register the Logger, LoggerFactory, and TestMiddleware
        ServiceConfigurator.Bind<ILogger>()
                           .WithInstance(Mock.Of<ILogger>());
        ServiceConfigurator.Bind<ILoggerFactory>()
                           .WithInstance(Mock.Of<ILoggerFactory>());
        ServiceConfigurator.Bind<TestMiddleware>()
                            .WithInstance(new TestMiddleware());

        List<Type> pipelineOrder = new();
        ServiceConfigurator.Bind<ITestMiddleware>()
                           .WithFactory(sp => new MiddlewareA(pipelineOrder))
                           .WithName("A");

        ServiceConfigurator.Bind<ITestMiddleware>()
                            .WithFactory(sp => new MiddlewareB(pipelineOrder))
                            .WithName("B");
        ServiceConfigurator.Bind<ITestMiddleware>()
                            .WithFactory(sp => new MiddlewareC(pipelineOrder))
                            .WithName("C");
        ServiceConfigurator.Bind<ITestMiddleware>()
                            .WithFactory(sp => new MiddlewareD(pipelineOrder))
                            .WithName("D");

        ServiceConfigurator.Register();
    }
}
