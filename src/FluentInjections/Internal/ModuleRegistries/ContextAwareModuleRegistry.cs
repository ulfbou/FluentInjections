namespace FluentInjections.Internal.Registries;

internal class ContextAwareModuleRegistry<TBuilder> : ModuleRegistry<TBuilder> where TBuilder : class
{
    private readonly string _currentContext;

    internal ContextAwareModuleRegistry(string contextName)
    {
        _currentContext = contextName;
    }

    /// <summary>
    /// Applies a <see cref="IServiceConfigurator"/> with contextual conditioning. 
    /// </summary>
    public void ApplyContextualServices(IServiceConfigurator serviceConfigurator)
    {
        foreach (var module in _serviceModules)
        {
            if (module is IContextAwareServiceModule contextModule &&
                !contextModule.ShouldRegisterForContext(_currentContext))
            {
                continue;
            }

            module.ConfigureServices(serviceConfigurator);
        }
    }

    public override bool CanHandle<TModule>() => typeof(IContextAwareServiceModule).IsAssignableFrom(typeof(TModule));
}
