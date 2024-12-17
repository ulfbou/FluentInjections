using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace FluentInjections.Tests
{
    public class MiddlewareConfiguratorTests
    {
        private class TestMiddleware : IMiddleware
        {
            public Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                return next(context);
            }
        }

        [Fact]
        public void Use_ValidMiddlewareType_AddsMiddleware()
        {
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builder);
            
            configurator.Use<TestMiddleware>();

            // Assert no exceptions thrown and middleware added (implementation-specific assertion)
        }

        [Fact]
        public void Use_InvalidMiddlewareType_ThrowsArgumentException()
        {
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builder);

            Assert.Throws<ArgumentException>(() => configurator.Use(typeof(string)));
        }

        [Fact]
        public void Use_UnsupportedBuilderType_ThrowsNotSupportedException()
        {
            var builder = new object(); // Unsupported builder type
            var configurator = new MiddlewareConfigurator<object>(builder);

            Assert.Throws<NotSupportedException>(() => configurator.Use<TestMiddleware>());
        }

        [Fact]
        public void Builder_ReturnsCorrectBuilderInstance()
        {
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builder);

            Assert.Equal(builder, configurator.Builder);
        }
    }
}