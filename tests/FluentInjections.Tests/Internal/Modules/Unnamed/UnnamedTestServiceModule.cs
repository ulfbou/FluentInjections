// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Internal.Modules.Unnamed;

public sealed class UnnamedTestServiceModule : Module<IServiceConfigurator>, IServiceModule
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<ITestService>()
                    .To<TestServiceWithDefaultValues>()
                    .AsSingleton();

        configurator.Bind<IOtherService>()
                    .To<OtherServiceImplementation>()
                    .AsScoped();

        configurator.Bind<IAnotherService>()
                    .To<AnotherServiceImplementation>()
                    .AsTransient();
        configurator.Bind<LoggingMiddleware>()
                    .AsSelf()
                    .AsSingleton();
        configurator.Bind<ErrorHandlingMiddleware>()
                    .AsSelf()
                    .AsSingleton();

        var loggerMock = new Mock<ILogger>();

        configurator.Bind<ILogger>()
                    .WithInstance(loggerMock.Object)
                    .AsSingleton();
    }
}
