using Autofac.Core.Registration;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Internal.Configurators;

public class MiddlewareConfigurator : IMiddlewareConfigurator
{
    private IServiceCollection _services;
    private IComponentRegistryBuilder _componentRegistry;

    public MiddlewareConfigurator(IServiceCollection services, IComponentRegistryBuilder componentRegistry)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
    }

    public object Middleware => throw new NotImplementedException();

    public Type MiddlewareType => throw new NotImplementedException();

    public void ApplyGroupPolicy(string groupName, Action<IMiddlewareBinding> configure) => throw new NotImplementedException();
    public void ConfigureAll(Action<IMiddlewareBinding> configure) => throw new NotImplementedException();
    public IMiddlewareBinding GetMiddleware<TMiddleware>() where TMiddleware : class => throw new NotImplementedException();
    public IMiddlewareBinding GetMiddleware(Type middleware) => throw new NotImplementedException();
    public void Register() => throw new NotImplementedException();
    public IMiddlewareBinding RemoveMiddleware<TMiddleware>() where TMiddleware : class => throw new NotImplementedException();
    public IMiddlewareBinding RemoveMiddleware(Type middleware) => throw new NotImplementedException();

    public void Use<TMiddleware>()
    {
        throw new NotImplementedException();
    }

    public IMiddlewareBinding UseMiddleware<TMiddleware>() where TMiddleware : class => throw new NotImplementedException();
    public IMiddlewareBinding UseMiddleware(Type middleware) => throw new NotImplementedException();
}
