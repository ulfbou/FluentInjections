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

namespace FluentInjections.Tests.MiddlewareConfiguratorTests
{
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

        private class TestMiddleware : IMiddleware
        {
            public Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                return next(context);
            }
        }
    }
}
