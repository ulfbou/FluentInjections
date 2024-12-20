using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections
{
    /// <summary>
    /// Represents a service configurator that provides methods to bind and manage services in the service collection.
    /// </summary>
    public interface IServiceConfigurator
    {
        /// <summary>
        /// Binds a service to the service collection.
        /// </summary>
        /// <returns>An interface for further configuring the service binding.</returns>
        IServiceBinding<TService> Bind<TService>() where TService : class;

        /// <summary>
        /// Unbinds a service of the specified type from the service collection.
        /// </summary>
        /// <typeparam name="TService">The type of the service to unbind.</typeparam>
        void Unbind<TService>();
    }
}