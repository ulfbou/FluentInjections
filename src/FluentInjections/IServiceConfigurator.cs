using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

/// <summary>
/// Represents a service configurator that provides methods to configure services within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define service configurations.
/// </remarks>
public interface IServiceConfigurator : IConfigurator<IServiceBinding>
{
    /// <summary>
    /// Binds a service to the service collection.
    /// </summary>
    /// <returns>An interface for further configuring the service binding.</returns>
    IServiceBinding<TService> Bind<TService>() where TService : class, new();

    /// <summary>
    /// Unbinds a service of the specified type from the service collection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to unbind.</typeparam>
    void Unbind<TService>();
}