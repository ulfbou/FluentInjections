# FluentInjections

FluentInjections is a powerful library for managing dependency injection and middleware configuration in .NET 9 applications. It leverages reflection to automatically register service and middleware modules, allowing for complex injection scenarios to be handled with ease using a fluent API.

## Features

- **Automatic Registration**: Automatically registers service and middleware modules from specified assemblies.
- **Fluent API**: Provides a fluent interface for defining service and middleware configurations.
- **Modular Design**: Supports modular service and middleware registration.
- **Extensibility**: Easily extendable to support custom configurations and module registries.

## Installation

To install FluentInjections, use the NuGet package manager:

```bash
dotnet add package FluentInjections
```

## Usage

### Adding FluentInjections to the Service Collection

To add FluentInjections to your service collection in a .NET 9 application, use the `AddFluentInjections` extension method. This method will scan the specified assemblies for service and middleware modules and register them automatically.

```csharp
using FluentInjections;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add FluentInjections to the service collection
builder.Services.AddFluentInjections<IApplicationBuilder>(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Use FluentInjections in the application pipeline
app.UseFluentInjections(Assembly.GetExecutingAssembly());

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Using FluentInjections in the Application Pipeline

To use FluentInjections in your application pipeline, call the `UseFluentInjections` extension method on the `IApplicationBuilder` instance. This will apply all registered middleware modules from the specified assemblies.

```csharp
using FluentInjections;
using Microsoft.AspNetCore.Builder;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Use FluentInjections in the application pipeline
app.UseFluentInjections(Assembly.GetExecutingAssembly());

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Example: Complex Injections with Fluent API

FluentInjections enables complex injections with the fluent API, allowing you to configure services and middleware with various options:

#### Define a Service Module

Create a new class `MyServiceModule.cs` to define a service module:

```csharp
using FluentInjections;
using Microsoft.Extensions.DependencyInjection;

public class MyServiceModule : IServiceModule
{
    public void ConfigureServices(IServiceConfigurator serviceConfigurator)
    {
        serviceConfigurator.Bind<IMyService>()
            .To<MyService>()
            .AsSingleton()
            .WithParameters(new { Param1 = "value1", Param2 = 42 })
            .Configure(service => service.Initialize())
            .Register();
    }
}

public interface IMyService
{
    void DoSomething();
    void Initialize();
}

public class MyService : IMyService
{
    private readonly string _param1;
    private readonly int _param2;

    public MyService(string param1, int param2)
    {
        _param1 = param1;
        _param2 = param2;
    }

    public void DoSomething()
    {
        // Implementation
    }

    public void Initialize()
    {
        // Initialization logic
    }
}
```

#### Define a Middleware Module

Create a new class `MyMiddlewareModule.cs` to define a middleware module:

```csharp
using FluentInjections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class MyMiddlewareModule : IMiddlewareModule<IApplicationBuilder>
{
    public void ConfigureMiddleware(IMiddlewareConfigurator<IApplicationBuilder> middlewareConfigurator)
    {
        middlewareConfigurator.UseMiddleware<MyMiddleware>()
            .WithPriority(1)
            .WithExecutionPolicy(policy => policy.RetryCount = 3)
            .WithMetadata(new { Description = "Sample Middleware" })
            .InGroup("Group1")
            .Enable()
            .Register();
    }
}

public class MyMiddleware
{
    private readonly RequestDelegate _next;

    public MyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Middleware logic
        await _next(context);
    }
}
```

### Advanced Scenarios

#### Conditional Module Registration

FluentInjections supports conditional module registration based on runtime conditions:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentInjections<IApplicationBuilder>()
    .RegisterModule<MyServiceModule>(() => DateTime.Now.DayOfWeek == DayOfWeek.Monday);
```

#### Factory-Based Module Registration

You can also register modules using a factory method:

```csharp
builder.Services.AddFluentInjections<IApplicationBuilder>()
    .RegisterModule(() => new MyServiceModule(), module => module.ConfigureSomeSettings());
```

### Full Range of Configurators and Binding Interfaces

#### ServiceConfigurator

The `ServiceConfigurator` provides various methods to bind services with different configurations:

- `Bind<TService>().To<TImplementation>()`: Binds a service to an implementation.
- `AsSingleton()`, `AsScoped()`, `AsTransient()`: Sets the service lifetime.
- `WithLifetime(ServiceLifetime)`: Sets a custom service lifetime.
- `WithInstance(TService)`: Binds a service to a specific instance.
- `WithFactory(Func<IServiceProvider, TService>)`: Binds a service using a factory.
- `WithParameters(object)`: Binds a service with parameters.
- `WithName(string)`: Binds a service with a name.
- `Configure(Action<TService>)`: Configures the service after creation.
- `ConfigureOptions<TOptions>(Action<TOptions>)`: Configures options for the service.

#### MiddlewareConfigurator

The `MiddlewareConfigurator` provides methods to configure middleware with various options:

- `UseMiddleware<TMiddleware>()`: Registers middleware.
- `WithPriority(int)`, `WithPriority(Func<int>)`: Sets the middleware priority.
- `WithExecutionPolicy<T>(Action<T>)`: Sets the execution policy for the middleware.
- `WithMetadata<TMetadata>(TMetadata)`: Attaches metadata to the middleware.
- `WithFallback(Func<TMiddleware, Task>)`: Sets a fallback for the middleware.
- `WithOptions<TOptions>(TOptions)`: Sets options for the middleware.
- `WithTag(string)`: Tags the middleware.
- `When(Func<bool>)`, `When<TContext>(Func<TContext, bool>)`: Sets a condition for the middleware.
- `InGroup(string)`: Groups middleware together.
- `DependsOn<TOtherMiddleware>()`, `Precedes<TPrecedingMiddleware>()`, `Follows<TFollowingMiddleware>()`: Sets dependencies for the middleware.
- `Disable()`, `Enable()`: Enables or disables the middleware.
- `RequireEnvironment(string)`: Sets the required environment for the middleware.
- `WithTimeout(TimeSpan)`: Sets a timeout for the middleware.
- `OnError(Func<Exception, Task>)`: Sets an error handler for the middleware.

## Best Practices

- **Assembly Scanning**: Ensure that all necessary assemblies are included in the scanning process to avoid missing any service or middleware modules.
- **Parameter Injection**: Use the `WithParameters` method to inject parameters into services as needed.
- **Modular Design**: Organize your services and middleware into modules to maintain a clean and maintainable codebase.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request to contribute.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Acknowledgments

Special thanks to all contributors and users of FluentInjections. Your support and feedback are greatly appreciated.

For more information, visit the [FluentInjections GitHub repository](https://github.com/ulfbou/FluentInjections).