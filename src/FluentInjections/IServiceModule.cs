namespace FluentInjections;

public interface IServiceModule
{
    void ConfigureServices(IServiceConfigurator configurator);
}
