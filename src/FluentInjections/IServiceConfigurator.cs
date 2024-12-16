using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections
{
    /// <summary>
    /// Represents a service configurator.
    /// </summary>
    public interface IServiceConfigurator
    {
        /// <summary>
        /// Binds a service to the service collection.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The service binding.</returns>
        IServiceBinding<TService> Bind<TService>() where TService : class;

        ///// <summary>
        ///// Adds a service to the service collection.
        ///// </summary>
        ///// <typeparam name="TService">The service type.</typeparam>
        ///// <typeparam name="TImplementation">The implementation type.</typeparam>
        ///// <param name="lifetime">The service lifetime.</param>
        //IServiceConfigurator AddService<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        //    where TService : class
        //    where TImplementation : class, TService;

        ///// <summary>
        ///// Adds a singleton service to the service collection.
        ///// </summary>
        ///// <typeparam name="TService">The service type.</typeparam>
        //IServiceConfigurator AddSingleton<TService>(TService implementationInstance) where TService : class;

        ///// <summary>
        ///// Adds a scoped service to the service collection.
        ///// </summary>
        ///// <typeparam name="TService">The service type.</typeparam>
        ///// <typeparam name="TImplementation">The implementation type.</typeparam>
        //IServiceConfigurator AddScoped<TService, TImplementation>() where TService : class where TImplementation : class, TService;

        ///// <summary>
        ///// Adds a transient service to the service collection.
        ///// </summary>
        //IServiceConfigurator AddTransient<TService, TImplementation>() where TService : class where TImplementation : class, TService;

        ///// <summary>
        ///// Configures options.
        ///// </summary>
        ///// <typeparam name="TOptions">The options type.</typeparam>
        //IServiceConfigurator ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class;
    }
}