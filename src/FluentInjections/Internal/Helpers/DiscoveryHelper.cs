// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Reflection;

namespace FluentInjections.Internal.Helpers;

internal static class DiscoveryHelper
{
    public static List<Type> DiscoverModules<TConfigurator>(params Assembly[]? assemblies)
    {
        var configuratorType = typeof(IModule<>).MakeGenericType(typeof(TConfigurator));
        assemblies ??= AppDomain.CurrentDomain.GetAssemblies();
        var moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => configuratorType.IsAssignableFrom(type) && !type.IsAbstract)
            .ToList();

        return moduleTypes;
    }
}
