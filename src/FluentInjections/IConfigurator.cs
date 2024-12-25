namespace FluentInjections;

/// <summary>
/// A marker interface that represents a configurator that provides methods to configure components within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define component configurations.
/// </remarks>
public interface IConfigurator
{
    /// <summary>
    /// Registers a service binding with the service collection.
    /// </summary>
    void Register();
}
