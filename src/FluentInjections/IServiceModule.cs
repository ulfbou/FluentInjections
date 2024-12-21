using FluentInjections.Internal.Descriptors;

namespace FluentInjections;

/// <summary>
/// Represents a service configurator that provides methods to configure services within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define service configurations.
/// </remarks>
public interface IServiceConfigurator : IConfigurator<IServiceBinding>
{
}

/// <summary>
/// Represents a module that can be registered with a configurator.
/// </summary>
/// <typeparam name="TConfigurator">The type of the configurator used to configure the module.</typeparam>
/// <remarks>
/// This interface should be implemented by classes that define registrations and configurations.
/// </remarks>
public interface IModule<TConfigurator> where TConfigurator : IConfigurator
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

/// <summary>
/// Represents a module that can be registered with a middleware configurator.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define middleware registrations and configurations.
/// </remarks>
public interface IMiddlewareModule : IModule<IMiddlewareConfigurator>
{
}

/// <summary>
/// Represents a module that can be registered with a service configurator.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define service registrations and configurations.
/// </remarks>
public interface IServiceModule : IModule<IServiceConfigurator>
{
}
