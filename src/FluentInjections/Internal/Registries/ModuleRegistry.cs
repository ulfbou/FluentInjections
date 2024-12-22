using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;

namespace FluentInjections.Internal.Registries;

/// <summary>
/// Represents a registry for service and middleware modules.
/// </summary>
internal class ModuleRegistry : IModuleRegistry
{
    public IModuleRegistry Register<TConfigurator>(IModule<TConfigurator> module) where TConfigurator : IConfigurator => throw new NotImplementedException();
    public IModuleRegistry Register<TConfigurator>(Func<TConfigurator> factory, Action<TConfigurator>? configure = null) where TConfigurator : IConfigurator => throw new NotImplementedException();
    public IModuleRegistry Unregister<TConfigurator>(IModule<TConfigurator> module) where TConfigurator : IConfigurator => throw new NotImplementedException();
    public IModuleRegistry Apply<TConfigurator>(TConfigurator configurator) where TConfigurator : IConfigurator => throw new NotImplementedException();
    public IModuleRegistry Initialize() => throw new NotImplementedException();
    public bool CanHandle<TConfigurator>() where TConfigurator : IConfigurator => throw new NotImplementedException();
    public bool CanHandle(Type type) => throw new NotImplementedException();
}
