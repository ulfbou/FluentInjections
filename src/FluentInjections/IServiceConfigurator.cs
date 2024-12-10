using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public interface IServiceConfigurator
{
    IServiceConfigurator AddService<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TService : class
        where TImplementation : class, TService;
    IServiceConfigurator AddSingleton<TService>(TService implementationInstance) where TService : class;

    IServiceConfigurator ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class;

    IServiceConfigurator AddTransient<TService, TImplementation>() where TService : class where TImplementation : class, TService;
}