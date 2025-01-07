// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections.Tests.Internal.Configurators;

public interface ITestServiceConfigurator : IServiceConfigurator
{
    IEnumerable<ServiceBindingDescriptor> GetDescriptors();
}
