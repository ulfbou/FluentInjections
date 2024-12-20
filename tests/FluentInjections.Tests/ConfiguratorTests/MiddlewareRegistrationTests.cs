using Autofac.Core;

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal;
using FluentInjections.Tests.Internal.Helpers;
using FluentInjections.Tests.Middlewares;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using Moq;

using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Channels;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FluentInjections.Tests.ConfiguratorTests;

public class MiddlewareRegistrationTests
{
    //Validate Middleware Registration
    //Test Objective: Verify that IApplicationBuilder calls UseMiddleware for your custom middleware.
    //Mock Setup:
    //Use a mocking library(e.g., Moq) to mock IApplicationBuilder.
    //Verify that the correct middleware type is added to the pipeline.

    [Fact]
    public void UseMiddleware_WithValidMiddleware_RegistersCorrectly()
    {
        // Arrange
        var mockApp = new Mock<IApplicationBuilder>();
        var app = mockApp.Object;

        // Act
        app.UseMiddleware<TestMiddleware>();

        // Assert
        mockApp.Verify(m => m.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    // Validate that options are correctly applied to middleware.
    // Mock Setup:
    // Create a mock options class and pass it to the configurator.
    // Assert that the middleware receives the correct configuration.
    [Fact]
    public void UseMiddleware_WithValidMiddlewareAndOptions_RegistersCorrectly()
    {
        // Arrange
        var mockApp = new Mock<IApplicationBuilder>();
        var app = mockApp.Object;

        // Act
        app.UseMiddleware<TestMiddleware>(new TestMiddlewareOptions { Option1 = "Test" });

        // Assert
        mockApp.Verify(m => m.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    // Validate how the middleware modifies HttpContext during a request.
    // Mock Setup:
    // Mock HttpContext with properties like Request and Response.
    // Mock any services the middleware uses.
    // Invoke the middleware method and verify:
    // Changes to headers, cookies, or request body.
    // Interactions with services.
    [Fact]
    public async Task TestMiddleware_InvokesCorrectly()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        var context = mockContext.Object;
        var mockRequest = new Mock<HttpRequest>();
        var request = mockRequest.Object;
        var mockResponse = new Mock<HttpResponse>();
        var response = mockResponse.Object;
        mockContext.SetupGet(c => c.Request).Returns(request);
        mockContext.SetupGet(c => c.Response).Returns(response);
        var middleware = new TestMiddleware();

        // Act
        await middleware.InvokeAsync(context, (c) =>
        {
            c.Response.StatusCode = (int)HttpStatusCode.OK;
            return Task.CompletedTask;
        });

        // Assert
        mockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.OK, Times.Once);
    }

    private class TestMiddlewareOptions
    {
        public string Option1 { get; set; }
    }
}
