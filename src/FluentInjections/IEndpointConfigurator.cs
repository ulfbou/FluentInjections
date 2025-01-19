// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Represents an endpoint configurator that provides methods to configure endpoints within the application.
/// </summary>
public interface IEndpointConfigurator : IConfigurator<IEndpointBinding>
{
    /// <summary>
    /// Maps an endpoint to the specified pattern, method and handler.
    /// </summary>
    /// <typeparam name="TService">The type of service to map.</typeparam>
    /// <typeparam name="TRequest">The type of request to map.</typeparam>
    /// <param name="pattern">The pattern to map.</param>
    /// <param name="method">The method to map.</param>
    /// <param name="handler">The handler to map.</param>
    /// <returns>The current instance of the <see cref="IEndpointsBuilder{TService, TRequest}"/>.</returns>
    IEndpointsBuilder<TService, TRequest> Map<TService, TRequest>(
        string pattern,
        EndpointMethod method,
        Func<TService, TRequest, HttpContext, Task<IResult>> handler)
        where TService : class;
}
