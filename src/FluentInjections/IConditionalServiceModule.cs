namespace FluentInjections;

public interface IConditionalServiceModule : IServiceModule
{
    bool ShouldRegister(IServiceProvider serviceProvider);
}
