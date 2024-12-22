using FluentInjections.Internal.Descriptors;

namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a service configurator.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define service registrations and configurations.
/// </remarks>
public interface IServiceModule : IModule<IServiceConfigurator>
{
}
