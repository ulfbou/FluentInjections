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
