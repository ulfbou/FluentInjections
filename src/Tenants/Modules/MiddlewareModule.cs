using FluentInjections;

using Tenants.Middleware;

namespace Tenants.Modules;

public class MiddlewareModule : Module<IMiddlewareConfigurator>
{
    public override void Configure(IMiddlewareConfigurator configurator)
    {
        if (configurator.Application is WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.MapControllers();
            app.UseHttpsRedirection();

            app.MapGet("/weatherforecast", () =>
            {
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast(
                    Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC: Random.Shared.Next(-20, 55)
                ));
            })
            .WithName("GetWeatherForecast");
        }

        configurator.UseMiddleware<TenantMiddleware>();
    }
}
