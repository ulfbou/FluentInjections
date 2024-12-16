
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public interface IServiceBinding<TService> where TService : class
{
    IServiceBinding<TService> As<TImplementation>() where TImplementation : class, TService;
    IServiceBinding<TService> WithName(string name);
    IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime);
    void Register();
    IServiceBinding<TService> To<TImplementation>() where TImplementation : class, TService;
    IServiceBinding<TService> WithParameters(object parameters);
    IServiceBinding<TService> AsSelf();
    IServiceBinding<TService> WithInstance(TService instance);
    IServiceBinding<TService> WithFactory(Func<TService> factory);
    IServiceBinding<TService> Configure(Action<TService> configure);
    IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class;
    IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions> configure) where TOptions : class;
    IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions, IServiceConfigurator> configure) where TOptions : class;
}