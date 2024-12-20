using Microsoft.Extensions.Options;

namespace FluentInjections.Tests.Services;

public sealed class TestServiceWithOptions : ITestService
{
    public string Param1 { get; }
    public int Param2 { get; }

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
}
