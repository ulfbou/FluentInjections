using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Wrappers;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

namespace FluentInjections.Tests.Internal.Wrappers;

internal sealed class TestApplicationBuilderWrapper(IApplicationBuilder innerBuilder, IMiddlewareConfigurator configurator, ILogger<ApplicationBuilderWrapper> logger)
    : ApplicationBuilderWrapper(innerBuilder, configurator, logger)
{
    internal ConcurrentBag<MiddlewareDescriptor> GetMiddlewareDescriptors() => _middlewareDescriptors;
}
