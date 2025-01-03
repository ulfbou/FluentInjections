// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

internal sealed class TestAutofacMiddlewareConfigurator : AutofacMiddlewareConfigurator
{
    public TestAutofacMiddlewareConfigurator(ContainerBuilder container, ILogger<AutofacMiddlewareConfigurator> logger) : base(container, logger) { }

    internal void TestValidateBindings() => ValidateBindings();
}
