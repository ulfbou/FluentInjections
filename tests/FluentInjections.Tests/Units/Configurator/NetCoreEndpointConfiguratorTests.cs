using FluentInjections.Tests.Internal.Configurators;

namespace FluentInjections.Tests.Units.Configurator;

public class NetCoreEndpointConfiguratorTests
{
    private readonly InternalEndpointConfiguratorTests _internal = new InternalEndpointConfiguratorTests();

    public NetCoreEndpointConfiguratorTests()
    {
        _internal.Configurator = new TestNetCoreEndpointConfigurator(_internal.Fixture.App, _internal.Fixture.LoggerMock.Object);
    }

    [Fact]
    public void Map_ShouldRegisterEndpointWithCorrectPatternAndMethod()
    {
        _internal.Map_ShouldRegisterEndpointWithCorrectPatternAndMethod();
    }

    [Fact]
    public void Map_ShouldRegisterEndpointWithCorrectHandler()
    {
        _internal.SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode();
    }
}