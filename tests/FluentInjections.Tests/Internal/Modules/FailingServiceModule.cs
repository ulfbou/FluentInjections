using FluentInjections.Tests.Internal.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Tests.Internal.Modules;

public class FailingServiceModule : Module<IServiceConfigurator>
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<IFailingService>()
                    .To<FailingService>()
                    .AsSingleton();
    }
}
