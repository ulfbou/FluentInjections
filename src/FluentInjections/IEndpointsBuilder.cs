// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections;

public interface IEndpointsBuilder<TRequest>
{
    IEndpointsBuilder<TRequest> RequireAuthorization();
    IEndpointsBuilder<TRequest> ConfigureValidation<TValidationFilter>(Action<TValidationFilter>? configure = null)
        where TValidationFilter : class;
    IEndpointsBuilder<TRequest> WithName(string endpointName);
    IEndpointsBuilder<TRequest> WithGroup(string groupName);
    IEndpointsBuilder<TRequest> WithErrorHandler(Func<Exception, Task> errorHandler);
    IEndpointsBuilder<TRequest> WithPriority(int priority);
    IEndpointsBuilder<TRequest> WithTag(string tag);
    IEndpointsBuilder<TRequest> WithTimeout(TimeSpan timeout);

    /// <summary>
    /// Applies a policy to all endpoints in the specified group.
    /// </summary>
    void ApplyGroupPolicy(string groupName, Action<IEndpointDescriptor> configure);

    /// <summary>
    /// Configures all endpoints with the specified action.
    /// </summary>
    void ConfigureAll(Action<IEndpointDescriptor> configure);
}
