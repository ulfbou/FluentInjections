namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a <see cref="IServiceConfigurator"/>.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Configures the service module with a <see cref="IServiceConfigurator"/>.
    /// </summary>
    void ConfigureServices(IServiceConfigurator configurator);
}
