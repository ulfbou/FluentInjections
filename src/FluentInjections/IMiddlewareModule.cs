namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a middleware configurator.
/// </summary>
public interface IMiddlewareModule<TBuilder> where TBuilder : class
{
    /// <summary>
    /// Configures the middleware with a <see cref="IMiddlewareConfigurator{TBuilder}"/>.
    /// </summary>
    void ConfigureMiddleware(IMiddlewareConfigurator<TBuilder> configurator);
}
