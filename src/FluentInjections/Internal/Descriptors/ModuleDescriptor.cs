namespace FluentInjections.Internal.Descriptors;

public class ModuleDescriptor
{
    public required Type ModuleType { get; set; }
    public object? Instance { get; set; }
    public int Priority => Instance switch
    {
        IConfigurableModule configurableModule => configurableModule.Priority,
        _ => 0
    };

    public TModule? TryGet<TModule>() where TModule : class
    {
        return Instance as TModule;
    }
}
