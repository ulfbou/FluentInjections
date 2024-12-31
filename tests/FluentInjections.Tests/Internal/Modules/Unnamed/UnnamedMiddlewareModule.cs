// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Internal.Middlewares;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Modules.Unnamed;

public sealed class UnnamedMiddlewareModule : Module<IMiddlewareConfigurator>, IMiddlewareModule
{
    public override void Configure(IMiddlewareConfigurator configurator)
    {
        configurator.UseMiddleware<LoggingMiddleware>()
                    .WithPriority(1);
        configurator.UseMiddleware<ErrorHandlingMiddleware>()
                    .WithPriority(2);
    }
}
