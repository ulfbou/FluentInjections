// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Utility.Fixtures;

public interface IConfiguratorFixture<TConfigurator, TServices, TProvider>
    where TConfigurator : class, IConfigurator
    where TServices : class, IServiceCollection
{
    TServices Services { get; set; }
    TProvider? Provider { get; set; }
    Mock<ITestService> TestServiceMock { get; set; }
    Mock<ILogger<TConfigurator>> LoggerMock { get; set; }
    void Setup();
    void Cleanup();
}
