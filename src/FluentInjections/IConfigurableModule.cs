namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a configurator.
/// </summary>
/// <typeparam name="TConfigurator">The type of configurator used to configure the module.</typeparam>
/// <remarks>
/// This interface should be implemented by classes that define registrations and configurations.
/// </remarks>
public interface IConfigurableModule<TConfigurator> : IModule<TConfigurator>
    where TConfigurator : IConfigurator
{
    /// <summary>
    /// Configures the module using the provided <typeparamref name="TConfigurator"/>.
    /// </summary>
    /// <param name="configurator">The configurator used to configure the module.</param>
    /// <remarks>
    /// This method is called to register and configure components within the application.
    /// </remarks>
    void Configure(TConfigurator configurator);
}
