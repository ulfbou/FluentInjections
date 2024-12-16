namespace FluentInjections.Internal.ModuleRegistries;

internal class ContextAwareModuleRegistry<TBuilder> : ModuleRegistry<TBuilder>
{
    private readonly string _currentContext;

    public ContextAwareModuleRegistry(string contextName)
    {
        _currentContext = contextName;
    }

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
