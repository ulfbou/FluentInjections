using FluentInjections.Internal.Constants;
using FluentInjections.Tests.Internal.Constants;
namespace FluentInjections.Internal.Configurators;

internal class MiddlewareDescriptor
{
    public Type MiddlewareType { get; set; }

    public object Instance
    {
        get => _instance ?? throw new InvalidOperationException(ExceptionMessages.InstanceNotSet);
        set => _instance = value ?? throw new ArgumentNullException(nameof(value));
    }
    private object? _instance;

    public int Priority
    {
        get => _priority ?? DefaultValues.Priority;
        set => _priority = value;
    }
    private int? _priority;

    public string Group
    {
        get => _group ?? DefaultValues.Group;
        set => _group = value ?? throw new ArgumentNullException(nameof(value));
    }
    private string? _group;

    public string? RequiredEnvironment { get; set; }
    public object? ExecutionPolicy { get; set; }
    public Func<object, Task>? Fallback { get; set; }
    public object? Options { get; set; }
    public object? Metadata { get; set; }
    public string? Tag { get; set; }
    public Func<bool>? Condition { get; set; }

    public bool IsEnabled
    {
        get => _isEnabled ?? true;
        set => _isEnabled = value;
    }
    private bool? _isEnabled;

    public List<Type> Dependencies
    {
        get => _dependencies;
        set => _dependencies = value ?? throw new ArgumentNullException(nameof(value));
    }
    private List<Type> _dependencies = new();

    public List<Type>? PrecedingMiddleware
    {
        get => _precedingMiddleware;
        set => _precedingMiddleware = value;
    }
    private List<Type>? _precedingMiddleware = new();

    public List<Type>? FollowingMiddleware
    {
        get => _followingMiddleware;
        set => _followingMiddleware = value;
    }
    private List<Type>? _followingMiddleware = new();

    public TimeSpan? Timeout { get; set; }
    public Func<Exception, Task>? ErrorHandler { get; set; }
    internal Action<MiddlewareDescriptor>? Callback { get; set; }

    internal MiddlewareDescriptor(Type middlewareType)
    {
        MiddlewareType = middlewareType;
    }

    public override bool Equals(object? obj)
    {
        if (obj is MiddlewareDescriptor other)
        {
            return MiddlewareType == other.MiddlewareType &&
                Priority == other.Priority &&
                Group == other.Group;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MiddlewareType, Priority, Group);
    }
}
