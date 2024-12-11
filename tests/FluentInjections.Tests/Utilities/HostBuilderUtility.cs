using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using static FluentInjections.Tests.MiddlewareConfiguratorTests;

namespace FluentInjections.Tests.Utilities;

public class HostBuilderUtility
{
    public static IHostBuilder CreateHostBuilder() => new HostBuilder().ConfigureWebHost(webHost =>
        {
            webHost.UseTestServer().Configure(app =>
            {
                var configurator = new MiddlewareConfigurator<IApplicationBuilder>(app);
                configurator.Use<FirstMiddleware>();
                configurator.Use<SecondMiddleware>();
                app.Run(async context => { await context.Response.WriteAsync("Hello, world!"); });
            });
        })
        .ConfigureServices(services =>
        {
            services.AddFluentInjections<IHostBuilder>();
            services.AddTransient<FirstMiddleware>();
            services.AddTransient<SecondMiddleware>();
        });
}

