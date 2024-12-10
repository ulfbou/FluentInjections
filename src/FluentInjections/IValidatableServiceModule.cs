namespace FluentInjections;

public interface IValidatableServiceModule : IServiceModule
{
    void Validate(IServiceProvider serviceProvider);
}
