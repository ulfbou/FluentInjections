using FluentInjections.Internal.ServiceProvider;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Tests.Utility.Fixtures;

using ServiceDescriptor = FluentInjections.Internal.Descriptors.ServiceDescriptor;
using DotNetServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Units.Configurator;

internal sealed class InternalEndpointConfiguratorTests :
    EndpointConfiguratorTests<TestNetCoreEndpointConfigurator, ServiceCollection, NetCoreServiceProvider, EndpointConfiguratorFixture>
{
    internal override TestNetCoreEndpointConfigurator Configurator { get; set; }
    internal override NetCoreServiceProvider? Provider { get; set; }

    public InternalEndpointConfiguratorTests()
    {
        Configurator = new TestNetCoreEndpointConfigurator(WebApplication.CreateBuilder().Build(), new LoggerFactory().CreateLogger<TestNetCoreEndpointConfigurator>());
        BuildProvider();
    }

    internal override void BuildProvider()
    {
        Provider = new NetCoreServiceProvider(Configurator.App.Services, new Dictionary<string, Dictionary<Type, ServiceDescriptor>>());
    }
}

