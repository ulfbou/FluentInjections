using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;

namespace FluentInjections.Tests.Internal.Extensions;

public static class CallbackExtensions
{
    /// <summary>
    /// Adds a callback to the middleware binding.
    /// </summary>
    /// <param name="binding">The middleware binding to be called back.</param>
    /// <param name="callback">The callback action.</param>
    /// <returns>The middleware binding instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the binding type is invalid.</exception>
    internal static IMiddlewareBinding Callback(this IMiddlewareBinding binding, Action<MiddlewareBindingDescriptor> callback)
    {
        ArgumentGuard.NotNull(binding, nameof(binding));
        ArgumentGuard.NotNull(callback, nameof(callback));

        callback(binding.Descriptor);
        return binding;
    }

    /// <summary>
    /// Adds a callback to the type instance.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to be called back.</param>
    /// <param name="callback">The callback action.</param>
    /// <returns>The instance.</returns>
    internal static T Callback<T>(this T instance, Action<T> callback)
    {
        ArgumentGuard.NotNull(instance, nameof(instance));
        ArgumentGuard.NotNull(callback, nameof(callback));

        callback(instance);
        return instance;
    }

    /// <summary>
    /// Adds a callback to the type instance with data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="instance">The instance to be called back.</param>
    /// <param name="callback">The callback function.</param>
    /// <param name="data">The data to be passed to the callback function.</param>
    /// <returns>The instance.</returns>
    internal static TResponse Callback<TData, TResponse>(this TResponse instance, Func<TData, TResponse> callback, TData data)
    {
        ArgumentGuard.NotNull(instance, nameof(instance));
        ArgumentGuard.NotNull(callback, nameof(callback));
        ArgumentGuard.NotNull(data, nameof(data));

        callback(data);
        return instance;
    }
}
