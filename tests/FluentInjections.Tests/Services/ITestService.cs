namespace FluentInjections.Tests.Services;

public interface ITestService
{
    string Param1 { get; }
    int Param2 { get; }

    void DoSomething();
}
