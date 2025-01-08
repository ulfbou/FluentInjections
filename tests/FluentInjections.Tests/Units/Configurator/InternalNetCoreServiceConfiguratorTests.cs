// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Units.Configurator;

/// <summary>
/// Represents a class for testing <see cref="NetCoreServiceconfigurator"/> instances and <see cref="IServiceConfigurator"/> implementations.
/// </summary>
internal sealed class InternalNetCoreServiceConfiguratorTests
    : ServiceConfiguratorTests<TestNetCoreServiceConfigurator, ServiceCollection, NetCoreServiceProvider, ServiceConfiguratorFixture>
{
    public Mock<ILogger<TestNetCoreServiceConfigurator>> LoggerMock { get; set; }
    internal override TestNetCoreServiceConfigurator Configurator { get; set; }
    internal override NetCoreServiceProvider? Provider { get; set; }

    public InternalNetCoreServiceConfiguratorTests() : base()
    {
        LoggerMock = new Mock<ILogger<TestNetCoreServiceConfigurator>>();
        Configurator = new TestNetCoreServiceConfigurator(Services, LoggerMock.Object);
        Provider = Fixture.Provider;
    }

    internal override void BuildProvider()
    {
        Guard.Null(Provider, nameof(Provider));

        Provider = Configurator.BuildServiceProvider();
    }

    protected override IReadOnlyDictionary<string, object?> GetMetadata<TService>(string name)
        where TService : class
    {
        return Provider?.GetMetadata<TService>(name) ?? new Dictionary<string, object?>().AsReadOnly();
    }

    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    public override string? ToString() => base.ToString();
    protected override T? GetService<T>() where T : class => base.GetService<T>();
    protected override T GetRequiredService<T>() => base.GetRequiredService<T>();
    protected override object? GetRequiredNamedService<T>(string name) => base.GetRequiredNamedService<T>(name);

    protected override IReadOnlyDictionary<string, object?> GetMetadata<TService>()
        where TService : class
    {
        return Provider?.GetMetadata<TService>() ?? new Dictionary<string, object?>().AsReadOnly();
    }
}
