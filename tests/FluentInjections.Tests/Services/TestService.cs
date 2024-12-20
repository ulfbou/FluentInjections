namespace FluentInjections.Tests.Services;

public sealed class TestService : ITestService
{
    public string Param1 { get; }
    public int Param2 { get; }

    public TestService(string param1, int param2)
    {
        Param1 = param1;
        Param2 = param2;
    }

    public class TestServiceOptions
    {
        public string Param1 { get; set; } = string.Empty;
        public int Param2 { get; set; }
    }
}
