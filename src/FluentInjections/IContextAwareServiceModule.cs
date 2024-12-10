namespace FluentInjections;

public interface IContextAwareServiceModule : IServiceModule
{
    bool ShouldRegisterForContext(string contextName);
}
