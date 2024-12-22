using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace FluentInjections.Internal.Extensions;

internal static class RegistrationBuilderExtensions
{
    public static IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> AsReflectionActivator<TLimit, TActivatorData, TStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
    {
        if (registration is IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> reflectionBuilder)
        {
            return reflectionBuilder;
        }

        throw new InvalidOperationException("The builder could not be cast to IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle>.");
    }

    public static IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> WithParameters<TLimit, TStyle>(
        this IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> registration,
        IEnumerable<Parameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            registration = registration.WithParameter(parameter);
        }

        return registration;
    }
}
