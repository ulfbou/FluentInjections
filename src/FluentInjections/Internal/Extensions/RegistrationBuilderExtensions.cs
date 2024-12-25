using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace FluentInjections.Internal.Extensions;

internal static class RegistrationBuilderExtensions
{
    public static IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> AsReflectionActivator<TLimit, TActivatorData, TStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
    {
        if (registration.IsReflectionData())
        {
            return (registration as IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle>)!;
        }

        throw new InvalidOperationException("The builder does not contain reflection data.");
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

    public static bool IsReflectionData<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
    {
        return registration.ActivatorData is ReflectionActivatorData;
    }

    public static bool IsReflectionData<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, out ReflectionActivatorData reflectionData)
    {
        if (registration.ActivatorData is ReflectionActivatorData data)
        {
            reflectionData = data;
            return true;
        }

        reflectionData = default!;
        return false;
    }
}
