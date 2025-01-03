// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

internal sealed class TestAutofacServiceConfigurator : AutofacServiceConfigurator
{
    public TestAutofacServiceConfigurator(ContainerBuilder container, ILogger<AutofacServiceConfigurator> logger) : base(container, logger) { }
    internal void TestValidateBindings() => ValidateBindings();
}
