using FluentInjections.Validation;

namespace FluentInjections;

public abstract class Module<TConfigurator> : IConfigurableModule<TConfigurator> where TConfigurator : IConfigurator
{
    public Type ConfiguratorType { get; set; }

    public Module()
    {
        ConfiguratorType = typeof(TConfigurator);
    }

    /// <inheritdoc />
    public virtual bool CanHandle<T>() where T : IConfigurator => ConfiguratorType.IsAssignableFrom(typeof(T));

    /// <inheritdoc />
    public virtual bool CanHandle(Type configuratorType)
    {
        ArgumentGuard.NotNull(configuratorType, nameof(configuratorType));
        return ConfiguratorType.IsAssignableFrom(configuratorType);
    }

    public abstract void Configure(TConfigurator configurator);
}
