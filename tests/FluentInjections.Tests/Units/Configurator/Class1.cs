using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Utility.Fixtures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Tests.Units.Configurator;

public abstract partial class MiddlewareConfiguratorTests<TConfigurator, TContainer, TFixture> : ConfiguratorTests<TConfigurator, TContainer, TFixture>
    where TConfigurator : class, IMiddlewareConfigurator
    where TContainer : class
    where TFixture : class, IMiddlewareConfiguratorFixture<TConfigurator, TContainer>, new()
{
    [Fact]
    public void ConfigureAll_ShouldApplyConfigurationToAllMiddlewares()
    {
        // Arrange
        var middlewareBinding1 = Configurator!.UseMiddleware<MiddlewareA>();
        var middlewareBinding2 = Configurator!.UseMiddleware<MiddlewareB>();
        var configurator = Configurator as MiddlewareConfigurator<TContainer, IMiddlewareBinding>;

        // Act
        Configurator.ConfigureAll(binding => binding.Condition = () => true);

        // Assert
        Assert.True(middlewareBinding1.Descriptor.IsEnabled);
        Assert.True(middlewareBinding2.Descriptor.IsEnabled);
    }
}
