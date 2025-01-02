using Autofac;

using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;

namespace FluentInjections.Tests.Units.Configurator;

internal sealed class InternalAutofacMiddlewareConfiguratorTests
    : MiddlewareConfiguratorTests<AutofacMiddlewareConfigurator, ContainerBuilder, AutofacMiddlewareConfiguratorFixture>
{
    private IContainer? _container;
    private IComponentContext? _context;

    protected override T? GetService<T>() where T : class
    {
        if (_context is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetService<T>.");
        }
        return _context.ResolveOptional<T>();
    }

    protected override T GetRequiredService<T>()
    {
        if (_context is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredService<T>.");
        }
        return _context.Resolve<T>();
    }

    protected override object? GetRequiredNamedService<T>(string name)
    {
        if (_context is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetRequiredNamedService<T>.");
        }
        return _context.GetNamedRequiredService<T>(name);
    }
}
