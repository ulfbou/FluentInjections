using FluentInjections.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

public class TestFixture
{
    public ServiceProvider ServiceProvider { get; private set; }

    public TestFixture()
    {
        var services = new ServiceCollection();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Add Serilog to the logging pipeline
        services.AddLogging(builder => builder.AddSerilog());

        // Register ILogger<MiddlewarePipelineBuilder>
        services.AddTransient<ILogger<MiddlewarePipelineBuilder>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<MiddlewarePipelineBuilder>();
        });

        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();
    }
}
