using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;

using static FluentInjections.Internal.Configurators.ServiceConfigurator;

namespace FluentInjections.Tests.Units.Configurator;

#if false
public abstract class BaseTestServiceConfiguratorTests<TConfigurator, TFixture> : ConfiguratorTests<TConfigurator, TFixture>
    where TConfigurator : class, IServiceConfigurator
    where TFixture : class, IConfiguratorFixture<TConfigurator>, new()
{
    protected IServiceCollection Services => Fixture.Services;

    [Fact]
    public void Register_WithImplementationType_RegistersType()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .To<TestService>() as ServiceBinding<ITestService>;
        var descriptor = binding?.GetDescriptor();

        // Act
        Configurator.Register();
        var sp = Services.BuildServiceProvider();

        // Assert
        Assert.IsType<TestService>(sp.GetRequiredService<ITestService>());
    }

    [Fact]
    public void Register_WithInstance_RegistersInstance()
    {
        // Arrange
        var instance = new TestService();
        var binding = Configurator.Bind<ITestService>()
                                  .WithInstance(instance);

        // Act
        Configurator.Register();
        var sp = Services.BuildServiceProvider();

        // Assert
        Assert.Same(instance, sp.GetRequiredService<ITestService>());
    }

    [Fact]
    public void Register_WithFactory_RegistersFactory()
    {
        // Arrange
        var descriptor = new ServiceBindingDescriptor(typeof(ITestService));
        descriptor.Factory = provider => new TestService();

        // Act
        Configurator.Register();
        var sp = Services.BuildServiceProvider();

        // Assert
        Assert.IsType<TestService>(sp.GetRequiredService<ITestService>());
    }

    [Fact]
    public void Register_WithConfigure_CallsConfigure()
    {
        // Arrange
        var descriptor = new ServiceBindingDescriptor(typeof(ITestService));
        descriptor.ImplementationType = typeof(TestService);
        descriptor.Configure = instance => ((TestService)instance).Param2 = 42;

        // Act
        Configurator.Register();
        var sp = Services.BuildServiceProvider();

        // Assert
        Assert.Equal(42, sp.GetRequiredService<ITestService>().Param2);
    }

    [Fact]
    public void Register_WithKey_RegistersKeyedTestService()
    {
        // Arrange
        var descriptor = new ServiceBindingDescriptor(typeof(ITestService));
        descriptor.ImplementationType = typeof(TestService);
        descriptor.Key = "key";

        // Act
        Configurator.Register();
        var sp = Services.BuildServiceProvider();

        // Assert
        Assert.IsType<TestService>(sp.GetRequiredNamedService<ITestService>("key"));
    }

    [Fact]
    public void Register_WithMetadata_RegistersMetadata()
    {
        // Arrange
        var descriptor = new ServiceBindingDescriptor(typeof(ITestService));
        descriptor.ImplementationType = typeof(TestService);
        descriptor.Metadata = new Dictionary<string, object>
        {
            { "param1", "value1" },
            { "param2", 42 }
        };

        // Act
        Configurator.Register();
        var sp = Services.BuildServiceProvider();

        // Assert
        var service = sp.GetRequiredService<ITestService>();
        Assert.Equal("value1", service.Param1);
        Assert.Equal(42, service.Param2);
    }
}
#endif
