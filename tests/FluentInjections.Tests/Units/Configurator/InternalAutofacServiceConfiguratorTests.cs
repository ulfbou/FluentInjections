using Autofac;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Validation;

namespace FluentInjections.Tests.Units.Configurator;

internal sealed class InternalAutofacServiceConfiguratorTests
    : ServiceConfiguratorTests<AutofacServiceConfigurator, ContainerBuilder, AutofacServiceConfiguratorFixture>
{
    private readonly InternalNetCoreServiceConfiguratorTests _internalTests = new InternalNetCoreServiceConfiguratorTests();
    private IContainer? _container;
    private IComponentContext? _context;
    private IServiceCollection ServiceCollection { get; set; }

    public InternalAutofacServiceConfiguratorTests() : base()
    {
        ServiceCollection = new ServiceCollection();
    }

    protected override void BuildProvider()
    {
        Guard.NotNull(Container, nameof(Container));

        Container.Populate(ServiceCollection);
        _container = Container.Build();
        _context = _container.Resolve<IComponentContext>();
        Provider = new AutofacServiceProvider(_container);
    }

    protected override IReadOnlyDictionary<string, object> GetMetadata<TService>(string name)
    {
        if (_context is null)
        {
            throw new InvalidOperationException("ServiceProvider is not built yet. Ensure that BuildProvider is called prior to calling GetMetadata<TService>.");
        }

        return _context.GetMetadata<TService>(name);
    }

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
