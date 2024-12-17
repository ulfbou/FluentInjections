﻿using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

using Moq;

using System.Diagnostics;

namespace FluentInjections.Tests.ConfiguratorTests;

public class MiddlewareConfiguratorTests
{
    private readonly IServiceCollection _services;
    private readonly Mock<IApplicationBuilder> _appBuilderMock;
    private readonly MiddlewareConfigurator<IApplicationBuilder> _middlewareConfigurator;
    private readonly List<Type> _registeredMiddleware;

    public MiddlewareConfiguratorTests()
    {
        _services = new ServiceCollection();
        _registeredMiddleware = new List<Type>();
        _appBuilderMock = new Mock<IApplicationBuilder>();

        _appBuilderMock.Setup(a => a.ApplicationServices).Returns(_services.BuildServiceProvider());
        _appBuilderMock.Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Callback<Func<RequestDelegate, RequestDelegate>>(middleware =>
            {
                // Check the target object of the delegate for the middleware type
                var middlewareInstance = middleware.Target;
                var middlewareType = middlewareInstance?.GetType().DeclaringType;

                if (middlewareType != null)
                {
                    _registeredMiddleware.Add(middlewareType);
                }
                else
                {
                    _registeredMiddleware.Add(typeof(Func<RequestDelegate, RequestDelegate>));
                }

                // Debug log
                Debug.WriteLine($"Middleware added to pipeline: {middlewareType?.Name ?? "Unknown"}");
            });

        _middlewareConfigurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, _appBuilderMock.Object);
    }

    [Fact]
    public void Middleware_Should_Respect_Priority()
    {
        // Arrange
        _middlewareConfigurator.UseMiddleware<MiddlewareA>().WithPriority(2);
        _middlewareConfigurator.UseMiddleware<MiddlewareB>().WithPriority(1);

        // Debug: Log middleware descriptors before registering
        Debug.WriteLine("Before Register:");
        LogMiddlewareDescriptors();

        // Act
        _middlewareConfigurator.Register();

        // Get registered middleware types from configurator
        var registeredMiddlewareTypes = _middlewareConfigurator.GetRegisteredMiddlewareTypes();

        // Debug: Log registered middleware types
        Debug.WriteLine("Registered middleware types:");
        foreach (var type in registeredMiddlewareTypes)
        {
            Debug.WriteLine(type.Name);
        }

        // Assert
        Assert.Equal(2, registeredMiddlewareTypes.Count);
        Assert.Equal(typeof(MiddlewareB), registeredMiddlewareTypes[0]); // Priority 1
        Assert.Equal(typeof(MiddlewareA), registeredMiddlewareTypes[1]); // Priority 2
    }

    [Fact]
    public void Middleware_With_Condition_Should_Be_Conditional()
    {
        // Arrange
        _middlewareConfigurator.UseMiddleware<MiddlewareA>().When(() => false); // Will not be registered
        _middlewareConfigurator.UseMiddleware<MiddlewareB>().When(() => true);  // Will be registered

        // Debug: Log middleware descriptors before registering
        Debug.WriteLine("Before Register:");
        LogMiddlewareDescriptors();

        // Act
        _middlewareConfigurator.Register();

        // Get registered middleware types from configurator
        var registeredMiddlewareTypes = _middlewareConfigurator.GetRegisteredMiddlewareTypes();

        // Debug: Log registered middleware types
        Debug.WriteLine("Registered middleware types:");
        foreach (var type in registeredMiddlewareTypes)
        {
            Debug.WriteLine(type.Name);
        }

        // Assert
        Assert.Equal(1, registeredMiddlewareTypes.Count);
        Assert.Equal(typeof(MiddlewareB), registeredMiddlewareTypes[0]);
    }

    [Fact]
    public void Middleware_With_Dependencies_Should_Be_Ordered_Correctly()
    {
        // Arrange
        _middlewareConfigurator.UseMiddleware<MiddlewareA>()
            .DependsOn<MiddlewareB>()
            .WithPriority(2);

        _middlewareConfigurator.UseMiddleware<MiddlewareB>()
            .WithPriority(1);

        // Debug: Log middleware descriptors before registering
        Debug.WriteLine("Before Register (Dependencies):");
        LogMiddlewareDescriptors();

        // Act
        _middlewareConfigurator.Register();
        var registeredMiddlewareTypes = _middlewareConfigurator.GetRegisteredMiddlewareTypes();

        // Assert
        Assert.Equal(2, registeredMiddlewareTypes.Count);
        Assert.Equal(typeof(MiddlewareB), registeredMiddlewareTypes[0]);
        Assert.Equal(typeof(MiddlewareA), registeredMiddlewareTypes[1]);
    }

    [Fact]
    public void Middleware_With_Group_Should_Respect_Grouping()
    {
        // Arrange
        _middlewareConfigurator.UseMiddleware<MiddlewareA>().InGroup("Group1");
        _middlewareConfigurator.UseMiddleware<MiddlewareB>().InGroup("Group2");

        // Debug: Log middleware descriptors before registering
        Debug.WriteLine("Before Register (Groups):");
        LogMiddlewareDescriptors();

        // Act
        _middlewareConfigurator.Register();

        // Assert
        Assert.Collection(_registeredMiddleware,
            middleware => Assert.Equal(typeof(MiddlewareA), middleware),
            middleware => Assert.Equal(typeof(MiddlewareB), middleware));
    }

    private void LogMiddlewareDescriptors()
    {
        IEnumerable<MiddlewareDescriptor> descriptors = _middlewareConfigurator.GetMiddlewareDescriptors();

        Debug.WriteLine("Current Middleware Descriptors:");

        foreach (var descriptor in descriptors)
        {
            Debug.WriteLine($"MiddlewareType: {descriptor.MiddlewareType}, " +
                              $"Priority: {descriptor.Priority}, " +
                              $"Condition: {descriptor.Condition?.Invoke() ?? true}, " +
                              $"Group: {descriptor.Group}, " +
                              $"Dependencies: {string.Join(", ", descriptor.Dependencies ?? new List<Type>())}");
        }
    }

    // Middleware stubs
    private class MiddlewareA : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next) => await next(context);
    }
    private class MiddlewareB : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next) => await next(context);
    }
}