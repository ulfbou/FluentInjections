using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentInjections.Internal.Extensions;

internal static class EndpointConventionBuilderExtensions
{
    public static TBuilder MapVerb<TBuilder>(this TBuilder builder, string verb, string pattern, Action<RouteHandlerBuilder> configure)
        where TBuilder : IEndpointRouteBuilder
    {
        Guard.NotNull(builder, nameof(builder));
        Guard.NotNullOrEmpty(verb, nameof(verb));
        Guard.NotNullOrEmpty(pattern, nameof(pattern));
        Guard.NotNull(configure, nameof(configure));

        var methodBuilder = builder.MapMethods(pattern, new[] { verb }, configure);
        configure?.Invoke(methodBuilder);
        return builder;
    }
}
