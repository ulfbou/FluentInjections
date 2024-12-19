namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with an <see cref="IServiceConfigurator"/>.
/// This interface should be implemented by classes that define service registrations and configurations.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Configures the services for this module using the provided <see cref="IServiceConfigurator"/>.
    /// This method is called to register and configure services within the dependency injection container.
    /// </summary>
    /// <param name="configurator">The service configurator used to register and configure services.</param>
    void ConfigureServices(IServiceConfigurator configurator);
}