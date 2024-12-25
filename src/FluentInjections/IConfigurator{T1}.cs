namespace FluentInjections;

/// <summary>
/// A marker interface that represents a configurator that provides methods to configure components within the application.
/// </summary>
/// <remarks>
public interface IConfigurator<out TBinding> : IConfigurator where TBinding : IBinding { }
