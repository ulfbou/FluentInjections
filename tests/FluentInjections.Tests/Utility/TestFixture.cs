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

        // Register services here (if needed)

        // Create the logger instance
        var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<MiddlewarePipelineBuilder>();

        // Build the service provider with the logger
        services.AddTransient<MiddlewarePipelineBuilder>(provider => new MiddlewarePipelineBuilder(services, logger));
        ServiceProvider = services.BuildServiceProvider();
    }
}
