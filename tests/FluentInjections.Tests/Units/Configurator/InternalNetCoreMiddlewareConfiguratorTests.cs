using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace FluentInjections.Tests.Units.Configurator;

internal sealed class InternalNetCoreMiddlewareConfiguratorTests
    : MiddlewareConfiguratorTests<NetCoreMiddlewareConfigurator<ApplicationBuilder>, ServiceCollection, NetCoreMiddlewareConfiguratorFixture>
{
}