using FluentInjections.Tests.Constants;
using FluentInjections.Tests.Extensions;
using FluentInjections.Tests.Middleware;
using FluentInjections.Tests.Utilities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Tests.MiddlewareConfiguratorTests;

public class MiddlewarePipelineTests : IClassFixture<WebApplicationFactory<MiddlewarePipelineTests.Startup>>
{
    [Fact]
    public async Task CreatePipeline_ExecutesMiddlewaresInOrder()
    {
        // Arrange
        var middlewareMock1 = new Mock<IMiddleware>();
        var middlewareMock2 = new Mock<IMiddleware>();

        middlewareMock1
            .Setup(m => m.InvokeAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Returns<HttpContext, RequestDelegate>((context, next) => next(context));

        middlewareMock2
            .Setup(m => m.InvokeAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Returns<HttpContext, RequestDelegate>((context, next) => next(context));

        var middlewares = new[] { middlewareMock1.Object, middlewareMock2.Object };
        var finalHandler = new RequestDelegate(context =>
        {
            context.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        var pipeline = MiddlewarePipelineHelper.CreatePipeline(middlewares, finalHandler);
        var context = new DefaultHttpContext();

        // Act
        await pipeline(context);

        // Assert
        middlewareMock1.Verify(m => m.InvokeAsync(context, It.IsAny<RequestDelegate>()), Times.Once);
        middlewareMock2.Verify(m => m.InvokeAsync(context, It.IsAny<RequestDelegate>()), Times.Once);
    }

    [Fact]
    public async Task Build_CreatesPipelineThatInvokesMiddlewares()
    {
        // Arrange
        var middlewareMock1 = new Mock<IMiddleware>();
        var middlewareMock2 = new Mock<IMiddleware>();

        middlewareMock1
            .Setup(m => m.InvokeAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Returns<HttpContext, RequestDelegate>((context, next) => next(context));

        middlewareMock2
            .Setup(m => m.InvokeAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Returns<HttpContext, RequestDelegate>((context, next) => next(context));

        var middlewares = new[]
        {
            middlewareMock1.Object,
            middlewareMock2.Object
        };

        RequestDelegate finalHandler = context =>
        {
            context.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        var pipeline = middlewares.Reverse().Aggregate(finalHandler, (next, middleware) =>
            context => middleware.InvokeAsync(context, next));

        var context = new DefaultHttpContext();

        // Act
        await pipeline(context);

        // Assert
        middlewareMock1.Verify(m => m.InvokeAsync(context, It.IsAny<RequestDelegate>()), Times.Once);
        middlewareMock2.Verify(m => m.InvokeAsync(context, It.IsAny<RequestDelegate>()), Times.Once);
    }

    public sealed class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<TestMiddleware>();
        }
    }

    [Fact]
    public async Task Builder_WithMultipleNamedMiddleware_ExecutesInOrderAsync()
    {
        var iterationList = new List<string>();
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger<NamedMiddleware>>();
        services.AddSingleton(loggerMock.Object);

        var builder = new MiddlewarePipelineBuilder(services, new Mock<ILogger<MiddlewarePipelineBuilder>>().Object);
        builder.UseMiddleware<NamedMiddleware>(Counting.First, iterationList, loggerMock.Object);
        builder.UseMiddleware<NamedMiddleware>(Counting.Second, iterationList, loggerMock.Object);
        builder.UseMiddleware<NamedMiddleware>(Counting.Third, iterationList, loggerMock.Object);

        var pipeline = builder.Build();
        await pipeline.Invoke(new DefaultHttpContext());

        Assert.Equal(new[] { Counting.First, Counting.Second, Counting.Third }, iterationList);

        loggerMock.VerifyLog(LogLevel.Information, Counting.First);
        loggerMock.VerifyLog(LogLevel.Information, Counting.Second, Times.Once());
        loggerMock.VerifyLog(LogLevel.Information, Counting.Third, Times.Once());
    }

    private class TestMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return next(context);
        }
    }
}
