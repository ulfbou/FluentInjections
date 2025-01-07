// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Units.Configurator;

internal interface IInternalNetCoreServiceConfiguratorTests
{
    bool Equals(object? obj);
    int GetHashCode();
    string? ToString();
}

internal sealed class InternalNetCoreServiceConfiguratorTests
    : ServiceConfiguratorTests<NetCoreServiceConfigurator, ServiceCollection, NetCoreServiceProvider, ServiceConfiguratorFixture>, IInternalNetCoreServiceConfiguratorTests
{
    internal override NetCoreServiceConfigurator Configurator { get; set; }
    internal override NetCoreServiceProvider? Provider { get; set; }

    public InternalNetCoreServiceConfiguratorTests() : base()
    {
        Configurator = new NetCoreServiceConfigurator(Services, Fixture.LoggerMock.Object);
        Provider = Fixture.Provider;
    }

    internal override void BuildProvider()
    {
        if (Provider is not null)
        {
            throw new InvalidOperationException("Provider already built");
        }

        Provider = Configurator.BuildServiceProvider();
    }

    protected override IReadOnlyDictionary<string, object> GetMetadata<TService>(string name)
        where TService : class
    {
        return Provider?.GetMetadata<TService>(name) ?? new Dictionary<string, object>();
    }

    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    public override string? ToString() => base.ToString();
    protected override T? GetService<T>() where T : class => base.GetService<T>();
    protected override T GetRequiredService<T>() => base.GetRequiredService<T>();
    protected override object? GetRequiredNamedService<T>(string name) => base.GetRequiredNamedService<T>(name);
}
