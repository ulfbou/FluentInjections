using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Wrappers;

internal sealed class FluentInjectionsServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return services; // Just return the existing collection
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        // Build the inner provider FIRST
        var innerProvider = containerBuilder.BuildServiceProvider();

        // Now create your NetCoreServiceProvider, passing in the inner provider and named services
        return new NetCoreServiceProvider(innerProvider, NetCoreNamedExtensions.NamedServices);
    }
}
