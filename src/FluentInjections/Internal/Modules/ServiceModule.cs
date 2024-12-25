using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Internal.Modules;

public abstract class ServiceModule() : Module<IServiceConfigurator>(), IServiceModule
{
}
