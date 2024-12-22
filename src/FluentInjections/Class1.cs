using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

public static class FluentInjectionsExtensions
{
    /// <summary>
    /// Configures AutoFac as the DI container for the application and sets it as the default IServiceProvider.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="configure">A callback to configure the AutoFac container builder.</param>
    /// <returns>An IServiceProvider backed by AutoFac.</returns>
    internal static IServiceProvider UseAutoFacAsDefault(this IServiceCollection services, Action<ContainerBuilder> configure)
    {
        var builder = new ContainerBuilder();

        builder.Populate(services);

        configure?.Invoke(builder);

        var container = builder.Build();

        var sp = services.FirstOrDefault(sp => sp.ServiceType == typeof(IServiceProvider));

        if (sp is not null)
        {
            services.Remove(sp);
        }

        var serviceProvider = new AutofacServiceProvider(container);
        services.AddSingleton<IServiceProvider>(serviceProvider);

        return serviceProvider;
    }

    public static IServiceCollection AddFluentInjections(this IServiceCollection services, params Assembly[]? assemblies)
    {
        services.UseAutoFacAsDefault(builder =>
        {
            builder.RegisterModule(new FluentInjectionsModule(services));
        });

        return services;
    }
}
