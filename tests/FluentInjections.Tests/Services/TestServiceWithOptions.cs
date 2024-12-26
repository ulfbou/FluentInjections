using Microsoft.Extensions.Options;

namespace FluentInjections.Tests.Services;

internal sealed class TestServiceWithOptions : ITestService
{
    public string Param1 { get; private set; }
    public int Param2 { get; private set; }

    public TestServiceWithOptions(IOptions<TestServiceOptions> options)
    {
        Param1 = options.Value.Param1;
        Param2 = options.Value.Param2;
    }

    public class TestServiceOptions
    {
        public string Param1 { get; set; } = string.Empty;
        public int Param2 { get; set; }
    }

    public void DoSomething()
    {
        Param2 = -1;
    }
}
