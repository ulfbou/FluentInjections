using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Extensions;

public static class AppBuilderExtensions
{
    /// <summary>
    /// Terminates the RequestDelegate pipeline with a succesful status code.
    /// </summary>
    /// <param name="app">The application builder instance.</param>
    /// <returns>The application builder instance.</returns>
    internal static IApplicationBuilder SuccessCode(this IApplicationBuilder app)
    {
        app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Pipeline terminated successfully.");
            await next(context);
        });

        return app;
    }

    /// <summary>
    /// Runs the pipeline with the specified service provider.
    /// </summary>
    /// <param name="app">The application builder instance.</param></param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal static async Task RunPipelineAsync(this IApplicationBuilder app)
    {
        var sp = app.ApplicationServices;
        using (var scope = sp.CreateAsyncScope())
        {
            var context = new DefaultHttpContext();
            app.SuccessCode();
            var pipeline = app.Build();
            await pipeline(context);
        }
    }
}
