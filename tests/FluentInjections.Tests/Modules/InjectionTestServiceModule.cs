using FluentInjections.Tests.Services;

namespace FluentInjections.Tests.Modules;

public sealed class InjectionTestServiceModule() : Module<IServiceConfigurator>(), IServiceModule
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithParameters(new { param1 = "value1", param2 = 42 })
                    .WithName("Test42")
                    .AsSingleton();

        configurator.Bind<ITestService>()
                    .WithInstance(new TestService("value1", 43))
                    .WithName("Test43")
                    .AsSingleton();

        configurator.Bind<ITestService>()
                    .To<TestServiceWithOptions>()
                    .WithName("Test44")
                    .AsSingleton();

        configurator.Bind<ITestService>()
                    .To<TestServiceWithDefaultValues>()
                    .AsSingleton();
    }
}
