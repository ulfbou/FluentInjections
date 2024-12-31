using FluentInjections.Internal.Configurators;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Utility.Wrapper;

public class TestNetCoreServiceProviderWrapper : NetCoreServiceProvider
{
    public TestNetCoreServiceProviderWrapper(IServiceProvider serviceProvider, IDictionary<string, ServiceDescriptor> keyedServiceDescriptors)
        : base(serviceProvider, keyedServiceDescriptors) { }

    public void AddKeyedService<TService>(string key, ServiceDescriptor descriptor)
    {
        _keyedServiceDescriptors[key] = descriptor;
    }

    public void AddKeyedService<TService>(string key, TService instance)
    {
        Guard.NotNullOrWhiteSpace(key, nameof(key));
        Guard.NotNull(instance, nameof(instance));

        var descriptor = new ServiceDescriptor(typeof(TService), instance!);
        AddKeyedService<TService>(key, descriptor);
    }
}
