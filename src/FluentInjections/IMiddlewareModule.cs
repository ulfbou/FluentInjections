namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a middleware configurator.
/// This interface should be implemented by classes that define middleware registrations and configurations.
/// </summary>
/// <typeparam name="TBuilder">The type of the application builder.</typeparam>
public interface IMiddlewareModule<TBuilder> where TBuilder : class
{
    /// <summary>
    /// Configures the middleware for this module using the provided <see cref="IMiddlewareConfigurator{TBuilder}"/>.
    /// This method is called to register and configure middleware components within the application.
    /// </summary>
    /// <param name="configurator">The middleware configurator used to register and configure middleware components.</param>
    void ConfigureMiddleware(IMiddlewareConfigurator<TBuilder> configurator);
}