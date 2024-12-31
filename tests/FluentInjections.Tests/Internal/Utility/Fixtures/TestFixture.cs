// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

public class TestFixture
{
    public IServiceCollection Services { get; set; }
    public ServiceProvider ServiceProvider { get; private set; }

    public TestFixture()
    {
        Services = new ServiceCollection();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Add Serilog to the logging pipeline
        Services.AddLogging(builder => builder.AddSerilog());

        // Register services here (if needed)

        // Create the logger instance
        var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<MiddlewarePipelineBuilder>();

        // Build the service provider with the logger
        Services.AddTransient<MiddlewarePipelineBuilder>(provider => new MiddlewarePipelineBuilder(Services, logger));
        ServiceProvider = Services.BuildServiceProvider();
    }
}
