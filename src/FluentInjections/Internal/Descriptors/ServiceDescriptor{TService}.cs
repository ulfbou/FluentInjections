// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

namespace FluentInjections.Internal.Descriptors;

public class ServiceDescriptor<TService>(IServiceConfigurator configurator)
    : ServiceDescriptor(typeof(TService), configurator) where TService : notnull
{ }
