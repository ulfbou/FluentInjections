using FluentInjections;

using Tenants.Middleware;
using Tenants.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentInjections(typeof(ServiceModule).Assembly);

var app = builder.Build();

app.UseFluentInjections(typeof(MiddlewareModule).Assembly);
app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary = null)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
