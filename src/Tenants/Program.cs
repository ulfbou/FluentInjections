using FluentInjections;

using Tenants.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddFluentInjections(typeof(Program).Assembly);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/weatherforecast", () =>
{
    return Enumerable.Range(1, 5).Select(index => new WeatherForecast(
        Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC: Random.Shared.Next(-20, 55)
    ));
})
.WithName("GetWeatherForecast");

app.UseFluentInjections();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary = null)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
