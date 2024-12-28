// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

using Serilog;

using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace FluentInjections.Tests.Utility;
internal static class TestLogging
{
    internal static IServiceCollection RegisterLogger(IServiceCollection services)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Add Serilog to the logging pipeline
        services.AddLogging(builder => builder.AddSerilog());

        return services;
    }
}
