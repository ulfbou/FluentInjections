using Autofac.Core;

using FluentInjections;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;
using FluentInjections.Tests.ConfiguratorTests;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

using static FluentInjections.Tests.ConfiguratorTests.ServiceConfiguratorTests;

namespace FluentInjections.Tests.ModuleRegistryTests;

public class ServiceConfiguratorTests2
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceConfigurator _serviceConfigurator;

    public ServiceConfiguratorTests2()
    {
        _serviceConfigurator = new ServiceConfigurator(_services);
    }


    public interface ITestService
    {
    }

    public class TestService : ITestService
    {
        public string Param1 { get; }
        public int Param2 { get; }

        public TestService(string param1, int param2)
        {
            Param1 = param1;
            Param2 = param2;
        }
    }
}