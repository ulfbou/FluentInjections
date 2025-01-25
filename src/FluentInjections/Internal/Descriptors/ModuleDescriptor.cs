namespace FluentInjections.Internal.Descriptors;

public class ModuleDescriptor
{
    public required Type ModuleType { get; set; }
    public object? Instance { get; set; }
    public int Priority => Instance switch
    {
        IModule module => module.Priority,
        _ => int.MaxValue
    };

    public TModule? TryGet<TModule>() where TModule : class
    {
        return Instance as TModule;
    }
}
