namespace FluentInjections;

public interface IPrioritizedServiceModule : IServiceModule
{
    int Priority { get; }
}
