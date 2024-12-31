// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Internal.Services;

using Moq;

namespace FluentInjections.Tests.Internal.Modules;

public sealed class NamedTestServiceModule : Module<IServiceConfigurator>, IServiceModule
{
    internal Mock<ITestService> Test43Mock { get; }

    public NamedTestServiceModule() : base()
    {
        Test43Mock = new Mock<ITestService>();
        Test43Mock.Setup(x => x.Param1).Returns("value1");
        Test43Mock.Setup(x => x.Param2).Returns(43);
    }

    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithParameters(new { param1 = "value1", param2 = 42 })
                    .WithName("Test42")
                    .AsSingleton();

        configurator.Bind<ITestService>()
                    .WithInstance(Test43Mock.Object)
                    .WithName("Test43")
                    .AsSingleton();

        //configurator.Bind<ITestService>()
        //            .To<TestServiceWithOptions>()
        //            .WithKey("Test44")
        //            .AsSingleton();
    }
}
