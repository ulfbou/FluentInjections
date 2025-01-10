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

using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentInjections(typeof(GreetingServiceModule).Assembly);

internal class GreetingServiceModule : Module<IServiceConfigurator>
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<IGreetingService>()
                    .To<GreetingService>()
                    .WithName("GreetingService");
    }
}

public interface IGreetingService
{
    Task<string> WriteGreeting();
    void SetName(string name);
    void SetAge(int age);
}

internal class GreetingService : IGreetingService
{
    private string? _name;
    private int _age;

    public void SetAge(int age) => _age = age;
    public void SetName(string name) => _name = name;

    public Task<string> WriteGreeting()
    {
        var name = _name!;
        var age = _age;
        return Task.FromResult($"Hello, {name}! You are almost {age} billion years old.");
    }
}
```

This example demonstrates how to use FluentInjections to register a service module (`GreetingServiceModule`) that binds an `IGreetingService` interface to a `GreetingService` implementation with the specific name "GreetingService". The `GreetingService` class provides methods to set the name and age of the person to greet and generate a greeting message.

### Using FluentInjections in the Application Pipeline

To use FluentInjections in your application pipeline, call the `UseFluentInjections` extension method on the `IApplicationBuilder` instance. This will apply all registered middleware modules from the specified assemblies.

```csharp
using FluentInjections;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

/* Register services as shown in the previous example */

var app = builder.Build();

app.UseFluentInjections(typeof(GreetingMiddlewareModule).Assembly);

app.MapGet("/greet", async (context) =>
{
    var greetingService = app.Services.GetNamedRequiredService<IGreetingService>("GreetingService");
    var greeting = await greetingService.WriteGreeting();
    await context.Response.WriteAsync(greeting);
});

app.Run();

/* Define the GreetingServiceModule, IGreetingService and GreetingService as shown in the previous example */

internal class GreetingMiddlewareModule : Module<IMiddlewareConfigurator>
{
    public override void Configure(IMiddlewareConfigurator configurator)
    {
        configurator.UseMiddleware<GreetingMiddleware>();
    }
}

internal class GreetingMiddleware : IMiddleware
{
    private readonly IGreetingService _service;

    public GreetingMiddleware(IGreetingService service)
    {
        _service = service;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _service.SetName("World");
        _service.SetAge(14);
        await next(context);
    }
}
```

### Example: Complex Injections with Fluent API

FluentInjections enables complex injections with the fluent API, allowing you to configure services and middleware with various options:

#### Define a Service Module

Create a new class `MyServiceModule.cs` to define a service module:

```csharp
using FluentInjections;

using Microsoft.AspNetCore.Http;

public class MyServiceModule : Module<IServiceConfigurator>
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<IMyService>()
                    .To<MyService>()
                    .Configure(service => service.Initialize())
                    .AsSingleton()
                    .WithParameters(new { Param1 = "value1", Param2 = 42 });
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

    public void DoSomething() {/* Implementation */ }
    public void Initialize() {/* Implementation */ }
}
```

#### Define a Middleware Module

Create a new class `MyMiddlewareModule.cs` to define a middleware module:

```csharp
using FluentInjections;

using Microsoft.AspNetCore.Http;

public class MyMiddlewareModule : Module<IMiddlewareConfigurator>
{
    public override void Configure(IMiddlewareConfigurator middlewareConfigurator)
    {
        middlewareConfigurator.UseMiddleware<MyMiddleware>()
                              .WithPriority(1)
                              .WithExecutionPolicy<IRetryPolicy>(policy => policy.RetryCount = 3)
                              .WithMetadata("Description", "Sample Middleware")
                              .InGroup("Group1")
                              .When(() => DateTime.Now.DayOfWeek == DayOfWeek.Monday);
        middlewareConfigurator.Register();
    }
}

public interface IRetryPolicy : IExecutionPolicy
{
    int RetryCount { get; set; }
}

public class MyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRetryPolicy _policy;

    public MyMiddleware(RequestDelegate next, IRetryPolicy policy)
    {
        _next = next;
        _policy = policy;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        /* Middleware logic */
        await _next(context);
    }
}
```

#### Code Breakdown

The provided code showcases two separate modules responsible for registering a custom middleware (`MyMiddleware`) and configuring its behavior:

##### 1. MyServiceModule

**Purpose:** Binds `MyMiddleware` to the `IHttpContextAccessor` service within the dependency injection container.

**Explanation:**
- `using FluentInjections;` imports the FluentInjections library.
- `using Microsoft.AspNetCore.Http;` provides access to the `IHttpContextAccessor` service.
- `public class MyServiceModule : Module<IServiceConfigurator>` inherits from the `Module<IServiceConfigurator>` class, defining a configuration module for services.
- `public override void Configure(IServiceConfigurator serviceConfigurator)` overrides the main configuration method.
- `serviceConfigurator.Bind<MyMiddleware>()` binds `MyMiddleware` to the `IHttpContextAccessor` service.
- `.AsSingleton()` sets the lifetime of the middleware instance to singleton, meaning a single instance will be shared across the application.
- `serviceConfigurator.Register();` registers the service configuration with the dependency injection container.

##### 2. MyMiddlewareModule

**Purpose:** Configures the behavior of `MyMiddleware` during the ASP.NET Core pipeline.

**Explanation:**
- `public class MyMiddlewareModule : Module<IMiddlewareConfigurator>` inherits from the `Module<IMiddlewareConfigurator>` class, defining a configuration module for middleware.
- `public override void Configure(IMiddlewareConfigurator middlewareConfigurator)` overrides the main configuration method.
- `middlewareConfigurator.UseMiddleware<MyMiddleware>()` specifies the middleware to be registered.
- `.WithPriority(1)` sets the execution priority of the middleware within the pipeline (lower numbers execute first).
- `.WithExecutionPolicy<IRetryPolicy>(policy => policy.RetryCount = 3)` configures a retry policy for the middleware. This example sets the retry count to 3 for failed requests.
- `.WithMetadata("Description", "Sample Middleware")` adds a description metadata to the middleware.
- `.InGroup("Group1")` assigns the middleware to a specific group (optional for future organization).
- `.When(() => DateTime.Now.DayOfWeek == DayOfWeek.Monday)` conditions the middleware execution only on Mondays.

##### MyMiddleware Class

**Purpose:** Processes HTTP requests and can leverage the injected `IHttpContextAccessor` for access to request/response context.

**Explanation:**
- `public interface IRetryPolicy : IExecutionPolicy` defines a basic interface for a retry policy (implementation not shown here).
- `public class MyMiddleware` represents the custom middleware.
- `private readonly RequestDelegate _next;` injects the next delegate in the middleware pipeline.
- `private readonly IRetryPolicy _policy;` injects the configured retry policy (optional, depends on your use case).
- `public MyMiddleware(RequestDelegate next, IRetryPolicy policy)` constructs the middleware with dependencies.
- `public async Task InvokeAsync(HttpContext context)` defines the asynchronous method that executes the middleware logic.
- `await _next(context);` calls the next delegate in the pipeline, allowing processing to continue.

##### Additional Notes

- The actual implementation of the retry logic would likely involve retrying the request within `InvokeAsync` based on the configured retry policy.
- Consider implementing additional middleware behavior specific to your application's needs.
- FluentInjections provides more features for dependency injection configuration. Refer to the library's documentation for details.

### Full Range of Configurators and Binding Interfaces

#### ServiceConfigurator

The `ServiceConfigurator` provides various methods to bind services with different configurations:

- `Bind(Type serviceType)`: Binds a service to an implementation.
- `Bind<TService>()`: Binds a service to an implementation with type safe inference.

By using Bind(Type serviceType), you can bind a service to an implementation with the following methods:

- `To(Type implementationType)`: Binds a service to an implementation.
- `AsSelf()`: Binds a service as itself. Must be a concrete type.
- `AsSingleton()`, `AsScoped()`, `AsTransient()`: Sets the service lifetime. 
- `WithLifetime(ServiceLifetime lifetime)`: Sets a custom service lifetime.
- `WithFactory(Func<IServiceProvider, object> factory)`: Binds a service using a factory.
- `WithName(string name)`: Binds a service with a name.
- `WithParameter(string key, object? value)`: Binds a service implementation type with a specific parameter.
- `WithParameters(object parameters)`: Binds a service implementation type with specific parameters.
- `WithParameters(IReadOnlyDictionary<string, object?> parameters)`: Binds a service implementation type with specific, named parameters.
- `WithInstance(object instance)`: Binds a service to a specific singleton instance.
- `WithMetadata(string name, object? value)`: Attaches metadata to the service, which can be used for additional configuration. Use `GetMetadata` in `IServiceProvider`.
- `Configure(Action<object> configure)`: Configures the service after creation.

By using Bind<TService>(), you can also bind a service to an implementation with the following methods:

- `To<TImplementation>() where TImplementation : class, TService`: Binds a service to an type safe implementation.
- `WithFactory(Func<IServiceProvider, TService> factory)`: Binds a service using a factory.
- `WithInstance(TService instance)`: Binds a service to a specific singleton instance of type TService.
- `Configure(Action<TService> configure)`: Configures a type safe service after creation with a lambda expression.

#### MiddlewareConfigurator

The `MiddlewareConfigurator` provides methods to configure middleware with various options. It has the following methods:

- `UseMiddleware(Type middlewareType)`: Registers middleware.
- `UseMiddleware<TMiddleware>()`: Registers middleware

By using `UseMiddleware(Type middlewareType)` or `UseMiddleware<TMiddleware>()`, you can register middleware with the following methods:

- `Instance { get; }`: Gets the middleware instance.
- `WithInstance(object instance)`: Sets the middleware instance.
- `WithName(string name)`: Sets the middleware name.
- `WithPriority(int priority)`: Sets the middleware priority.
- `WithPriority(Func<int> priority)`: Sets the middleware priority using a function.
- `WithPriority<TContext>(Func<TContext, int> priority)`: Sets the middleware priority using a function with context.
- `WithExecutionPolicy<TPolicy>(Action<TPolicy> value) where TPolicy : class`: Sets the execution policy for the middleware.
- `WithExecutionPolicy<TPolicy>(TPolicy policy) where TPolicy : class`: Sets the execution policy for the middleware.
- `WithMetadata(string name, object value)`: Attaches metadata to the middleware.
- `WithFallback(Func<object, Task> fallback)`: Sets a fallback for the middleware.
- `WithOptions<TOptions>(TOptions options) where TOptions : class`: Sets options for the middleware.
- `WithTag(string tag)`: Tags the middleware.
- `When(Func<bool> func)`: Sets a condition for the middleware.
- `When<TContext>(Func<TContext, bool> func)`: Sets a condition for the middleware.
- `InGroup(string group)`: Groups middleware together.
- `DependsOn<TOtherMiddleware>()`: Sets dependencies for the middleware.
- `Precedes<TPrecedingMiddleware>()`: Sets dependencies for the middleware.
- `Follows<TFollowingMiddleware>()`: Sets dependencies for the middleware.
- `WithTimeout(TimeSpan timeout)`: Sets a timeout for the middleware.
- `OnError(Func<Exception, Task> errorHandler)`: Sets an error handler for the middleware.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request to contribute.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Acknowledgments

Special thanks to all contributors and users of FluentInjections. Your support and feedback are greatly appreciated.

For more information, visit the [FluentInjections GitHub repository](https://github.com/ulfbou/FluentInjections).
