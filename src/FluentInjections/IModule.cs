namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a configurator.
/// </summary>
/// <typeparam name="TConfigurator">The type of the configurator used to configure the module.</typeparam>
/// <remarks>
/// This interface should be implemented by classes that define registrations and configurations.
/// </remarks>
public interface IModule<out TConfigurator>
    where TConfigurator : IConfigurator
{
    Type ConfiguratorType { get; set; }

    /// <summary>
    /// Determines whether the module can handle the specified configurator type.
    /// </summary>
    /// <typeparam name="T">The type of the configurator.</typeparam>
    /// <returns><see langword="true"/> if the module can handle the specified configurator type; otherwise, <see langword="false"/>.</returns>
    bool CanHandle<T>() where T : IConfigurator;

    /// <summary>
    /// Determines whether the module can handle the specified configurator type.
    /// </summary>
    /// <param name="configuratorType">The type of the configurator.</param>
    /// <returns><see langword="true"/> if the module can handle the specified configurator type; otherwise, <see langword="false"/>.</returns>
    bool CanHandle(Type configuratorType);
}
