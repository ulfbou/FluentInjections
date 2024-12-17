
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public interface IServiceBinding<TService> where TService : class
{
    IServiceBinding<TService> To<TImplementation>() where TImplementation : class, TService;
    IServiceBinding<TService> To(Type implementationType);
    IServiceBinding<TService> AsSelf();
    IServiceBinding<TService> AsSingleton();
    IServiceBinding<TService> AsScoped();
    IServiceBinding<TService> AsTransient();
    IServiceBinding<TService> WithFactory(Func<IServiceProvider, TService> factory);
    IServiceBinding<TService> WithName(string name);
    IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime);
    IServiceBinding<TService> WithParameters(object parameters);
    IServiceBinding<TService> WithParameters(IReadOnlyDictionary<string, object> parameters);
    IServiceBinding<TService> WithInstance(TService instance);
    IServiceBinding<TService> Configure(Action<TService> configure);
    IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class;
    IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions> configure) where TOptions : class;
    IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions, IServiceConfigurator> configure) where TOptions : class;
    void Register();
}