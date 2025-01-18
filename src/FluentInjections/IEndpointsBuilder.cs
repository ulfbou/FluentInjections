// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections;

public interface IEndpointsBuilder
{
    IEndpointsBuilder RequireAuthorization();
    IEndpointsBuilder UseValidation<TValidationFilter>(Action<TValidationFilter>? configure = null)
        where TValidationFilter : class;
    IEndpointsBuilder WithName(string endpointName);
    IEndpointsBuilder InGroup(string groupName);
    IEndpointsBuilder OnError(Func<Exception, Task> errorHandler);
    IEndpointsBuilder WithPriority(int priority);
    IEndpointsBuilder WithTag(string tag);
    IEndpointsBuilder WithTimeout(TimeSpan timeout);

    void ApplyGroupPolicy(string groupName, Action<IEndpointDescriptor> configure);
    void ConfigureAll(Action<IEndpointDescriptor> configure);
}
