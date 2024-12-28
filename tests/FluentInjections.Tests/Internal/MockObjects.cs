// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    internal static IMiddleware MockMiddlewareInstance => MockMiddleware.Object;
    private static Mock<IMiddleware> MockMiddleware => new Mock<IMiddleware>();

    internal static IServiceCollection MockServicesInstance => MockServices.Object;
    internal static Mock<IServiceCollection> MockServices => new Mock<IServiceCollection>();

    internal static IApplicationBuilder MockAppBuilderInstance => MockAppBuilder.Object;
    internal static Mock<IApplicationBuilder> MockAppBuilder => new Mock<IApplicationBuilder>();

    internal static HttpContext MockHttpContextInstance => MockHttpContext.Object;
    internal static Mock<HttpContext> MockHttpContext => new Mock<HttpContext>();

    internal static RequestDelegate MockRequestDelegateInstance => MockRequestDelegate.Object;
    internal static Mock<RequestDelegate> MockRequestDelegate => new Mock<RequestDelegate>();
}
