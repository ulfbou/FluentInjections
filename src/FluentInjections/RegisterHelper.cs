// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace FluentInjections;

internal static class RegisterHelper
{
    public static void RegisterModule<TConfigurator>(Type moduleType, Type interfaceType, TConfigurator configurator) where TConfigurator : IConfigurator
    {
        Debug.WriteLine($"Registering module {moduleType.Name} with configurator {typeof(TConfigurator).Name}");

        // Get the correct interface from the moduleType. Search for IConfigurableModule<T>
        var moduleInterface = moduleType.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurableModule<>));

        if (moduleInterface == null)
        {
            throw new InvalidOperationException($"Module {moduleType.FullName} does not implement IConfigurableModule<T>.");
        }

        //Get the T from IConfigurableModule<T>
        var moduleConfiguratorType = moduleInterface.GetGenericArguments().First();

        //Check if the ModuleConfigurator is assignable from TConfigurator
        if (!moduleConfiguratorType.IsAssignableFrom(typeof(TConfigurator))) // Swapped order for correct assignability check
        {
            throw new InvalidOperationException($"Configurator of type {typeof(TConfigurator).FullName} is not compatible with module {moduleType.FullName} which requires {moduleConfiguratorType.FullName}.");
        }

        var instance = Activator.CreateInstance(moduleType);
        var configureMethod = moduleType.GetMethod("Configure");

        if (configureMethod is null)
        {
            throw new InvalidOperationException($"No suitable Configure method found for module type {moduleType.Name}");
        }

        Debug.WriteLine($"Registering module {moduleType.Name} with configurator {moduleConfiguratorType.Name}");

        // Now invoke the Configure method using the provided configurator instance
        configureMethod.Invoke(instance, new object[] { configurator }); // Use the provided configurator
    }
}
