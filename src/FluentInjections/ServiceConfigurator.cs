using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FluentInjections;

public class ServiceConfigurator : IServiceConfigurator
{
    private readonly IServiceCollection _services;

    public ServiceConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public IServiceConfigurator AddService<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TService : class
        where TImplementation : class, TService
    {
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
        return this;
    }

    public IServiceConfigurator AddSingleton<TService>(TService implementationInstance) where TService : class
    {
        _services.AddSingleton(implementationInstance);
        return this;
    }

    public IServiceConfigurator ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class
    {
        _services.GroupJoin(_services, _ => true, _ => true, (_, services) =>
        {
            var options = services.OfType<TOptions>().FirstOrDefault();
            if (options is not null)
            {
                configure(options);
            }
            return services;
        });

        return this;
    }

    public IServiceConfigurator AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _services.AddTransient<TService, TImplementation>();
        return this;
    }
}
