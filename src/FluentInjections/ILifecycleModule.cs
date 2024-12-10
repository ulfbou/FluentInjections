namespace FluentInjections;

public interface ILifecycleModule : IDisposable
{
    void Initialize();
}
