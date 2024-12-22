using FluentInjections.Tests.Services;

namespace FluentInjections.Tests.Modules;

public sealed class InjectionTestServiceModule : IServiceModule
{
    public void ConfigureServices(IServiceConfigurator configurator)
    {
        configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithParameters(new { param1 = "value1", param2 = 42 })
                    .WithName("Test42")
                    .AsSingleton()
                    .Register();

        configurator.Bind<ITestService>()
                    .WithInstance(new TestService("value1", 43))
                    .WithName("Test43")
                    .AsSingleton()
                    .Register();

        // Registering a service with options
        configurator.Bind<ITestService>()
                    .To<TestServiceWithOptions>()
                    .WithName("Test44")
                    .AsSingleton()
                    .Configure<TestServiceWithOptions.TestServiceOptions>(options =>
                    {
                        options.Param1 = "value1";
                        options.Param2 = 44;
                    })
                    .Register();
    }
}
