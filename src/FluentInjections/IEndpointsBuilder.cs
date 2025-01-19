// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections;

public interface IEndpointsBuilder<TService, TRequest>
{
    IEndpointsBuilder<TService, TRequest> RequireAuthorization();
    IEndpointsBuilder<TService, TRequest> ConfigureValidation<TValidationFilter>(Action<TValidationFilter>? configure = null)
        where TValidationFilter : class;
    IEndpointsBuilder<TService, TRequest> WithName(string endpointName);
    IEndpointsBuilder<TService, TRequest> WithGroupName(string groupName);
    IEndpointsBuilder<TService, TRequest> WithErrorHandler(Func<Exception, Task> errorHandler);
    IEndpointsBuilder<TService, TRequest> WithPriority(int priority);
    IEndpointsBuilder<TService, TRequest> WithTag(string tag);
    IEndpointsBuilder<TService, TRequest> WithTimeout(TimeSpan timeout);
    IEndpointsBuilder<TService, TRequest> WithMetadata(string key, object value);

    /// <summary>
    /// Applies a policy to all endpoints in the specified group.
    /// </summary>
    void ApplyGroupPolicy(string groupName, Action<IEndpointDescriptor> configure);

    /// <summary>
    /// Configures all endpoints with the specified action.
    /// </summary>
    void ConfigureAll(Action<IEndpointDescriptor> configure);
}
