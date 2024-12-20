using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Tests.Internal;

internal static class MockObjects
{
    internal static IMock<IMiddleware> MockMiddleware => _mockMiddleware;
    internal static IMiddleware MockMiddlewareInstance => _mockMiddleware.Object;
    private static Mock<IMiddleware> _mockMiddleware = new Mock<IMiddleware>();

    internal static IServiceCollection MockServicesInstance => _mockServices.Object;
    internal static Mock<IServiceCollection> MockServices => _mockServices;
    private static Mock<IServiceCollection> _mockServices = new Mock<IServiceCollection>();

    internal static IApplicationBuilder MockApplicationBuilderInstance => _mockApp.Object;
    internal static Mock<IApplicationBuilder> MockApplicationBuilder => _mockApp;
    private static Mock<IApplicationBuilder> _mockApp = new Mock<IApplicationBuilder>();

    internal static HttpContext MockHttpContextInstance => _mockContext.Object;
    internal static Mock<HttpContext> MockHttpContext => _mockContext;
    private static Mock<HttpContext> _mockContext = new Mock<HttpContext>();

    internal static RequestDelegate MockRequestDelegateInstance => _mockRequestDelegate.Object;
    internal static Mock<RequestDelegate> MockRequestDelegate => _mockRequestDelegate;
    private static Mock<RequestDelegate> _mockRequestDelegate = new Mock<RequestDelegate>();
}
