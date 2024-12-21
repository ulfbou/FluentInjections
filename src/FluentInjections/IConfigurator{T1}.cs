namespace FluentInjections;

/// <summary>
/// A marker interface that represents a configurator that provides methods to configure components within the application.
/// </summary>
/// <remarks>
public interface IConfigurator<TBinding> : IConfigurator where TBinding : IBinding
{
}
