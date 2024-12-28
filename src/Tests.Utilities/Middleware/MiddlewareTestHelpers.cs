// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Utilities;

public static class MiddlewareTestHelpers
{
    public static ServiceProvider Builder(this IServiceCollection services)
    {
        return services.BuildServiceProvider();
    }

    public static HttpContext CreateHttpContext(ServiceProvider serviceProvider)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        return context;
    }
}
