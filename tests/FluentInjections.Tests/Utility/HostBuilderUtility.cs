using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentInjections.Tests.Utility;

public class HostBuilderUtility
{
    // TODO: Revisit completely
    public static IHostBuilder CreateHostBuilder(List<string> callOrder) => new HostBuilder().ConfigureWebHost(webHost =>
    {
        webHost.UseTestServer().Configure(app =>
        {
            //IServiceProvider sp = app.ApplicationServices.GetRequiredService<IServiceProvider>();
            //var configurator = new MiddlewareConfigurator<IApplicationBuilder>(app, sp);
            //configurator.Use<NamedMiddleware>("First Middleware", callOrder);
            //configurator.Use<NamedMiddleware>("Second Middleware", callOrder);
            //app.Run(async context => { await context.Response.WriteAsync("Hello, world!"); });
        });
    })
        .ConfigureServices(services =>
        {
            //services.AddFluentInjections<IHostBuilder>();
        });
}
