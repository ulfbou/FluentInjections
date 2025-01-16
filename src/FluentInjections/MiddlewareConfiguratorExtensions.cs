using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public static class MiddlewareConfiguratorExtensions
{
    public static IMiddlewareBinding UseMiddleware(this IMiddlewareConfigurator configurator, Type middlewareType)
    {
        Guard.NotNull(configurator, nameof(configurator));
        Guard.NotNull(middlewareType, nameof(middlewareType));

        return new NetCoreMiddlewareConfigurator.MiddlewareBinding(new MiddlewareBindingDescriptor(middlewareType, configurator));
    }

    public static IMiddlewareBinding UseMiddleware(this IMiddlewareConfigurator configurator, Func<RequestDelegate, RequestDelegate> middleware)
    {
        //Guard.NotNull(configurator, nameof(configurator));
        //Guard.NotNull(middleware, nameof(middleware));
        //return new NetCoreMiddlewareConfigurator.MiddlewareBinding(new MiddlewareBindingDescriptor(middleware.GetType(), configurator));
        throw new NotImplementedException();
    }
}
