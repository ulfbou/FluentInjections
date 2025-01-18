// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace FluentInjections;

public interface IEndpointConfigurator : IConfigurator<IEndpointBinding>
{
    IEndpointsBuilder<TRequest> Map<TService, TRequest>(
        string pattern,
        EndpointMethod method,
        Func<TService, TRequest, HttpContext, Task<IResult>> handler)
        where TService : class;
}
