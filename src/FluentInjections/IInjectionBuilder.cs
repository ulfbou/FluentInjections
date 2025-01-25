using FluentInjections.Internal.Utils;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

public interface IInjectionBuilder
{
    /// <summary>
    /// Registers assemblies that contains <see cref="IServiceModule"/> to be discovered. 
    /// </summary>
    /// <param name="assemblies">The assemblies to be discovered.</param>
    /// <returns>The <see cref="IInjectionBuilder"/> instance.</returns>
    IInjectionBuilder WithServices(params Assembly[]? assemblies);

    /// <summary>
    /// Registers assemblies that contains <see cref="IServiceModule"/> to be discovered.
    /// </summary>
    /// <param name="configureAssemblies">The action to configure the services.</param>
    /// <returns>The <see cref="IInjectionBuilder"/> instance.</returns>
    IInjectionBuilder WithServices(Action<AssemblyCollection> configureAssemblies);

    /// MIddleware assembly registration

    /// <summary>
    /// Registers assemblies that contains <see cref="IMiddlewareModule"/> to be discovered.
    /// </summary>
    /// <param name="assemblies">The assemblies to be discovered.</param>
    /// <returns>The <see cref="IInjectionBuilder"/> instance.</returns>
    public IInjectionBuilder WithMiddlewares(params Assembly[]? assemblies);

    /// <summary>
    /// Registers assemblies that contains <see cref="IMiddlewareModule"/> to be discovered.
    /// </summary>
    /// <param name="configureAssemblies">The action to configure the middlewares.</param>
    /// <returns>The <see cref="IInjectionBuilder"/> instance.</returns>
    public IInjectionBuilder WithMiddlewares(Action<AssemblyCollection> configureAssemblies);

    // Register ServiceCollection and ServiceProvider

    /// <summary>
    /// Registers the <see cref="IServiceCollection"/> to be used for service registration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to be used for service registration.</param>
    /// <returns>The <see cref="IInjectionBuilder"/> instance.</returns>
    IInjectionBuilder WithServiceCollection(IServiceCollection services);

    /// <summary>
    /// Registers the <see cref="IServiceProvider"/> to be used for service resolution.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to be used for service resolution.</param>
    /// <returns>The <see cref="IInjectionBuilder"/> instance.</returns>
    IInjectionBuilder WithServiceProvider(IServiceProvider serviceProvider);
}
