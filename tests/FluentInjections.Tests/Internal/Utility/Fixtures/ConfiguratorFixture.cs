// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions.Common;

using FluentInjections.Internal.Utils;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System.Configuration;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal abstract class ConfiguratorFixture<TConfigurator, TServices, TProvider> : IConfiguratorFixture<TConfigurator, TServices, TProvider>, IDisposable
    where TConfigurator : class, IConfigurator
    where TServices : class, IServiceCollection, new()
    where TProvider : class, IServiceProvider
{
    public TServices Services { get; set; }
    public abstract TConfigurator Configurator { get; set; }
    public TProvider? Provider { get; set; }
    public Mock<ITestService> MockTestService { get; set; }

    public ConfiguratorFixture()
    {
        Services = new TServices();
        MockTestService = new Mock<ITestService>();
    }

    public virtual void Setup()
    {
        Cleanup();
        Services = new TServices();
    }

    public virtual void Cleanup()
    {
        Services?.Clear();
        Services = default!;
    }

    public void Dispose()
    {
        Cleanup();
    }
}
