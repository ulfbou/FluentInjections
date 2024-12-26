using FluentInjections.Internal.Constants;

namespace FluentInjections.Internal.Descriptors;

public class MiddlewareBindingDescriptor
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
    public Type? OptionsType { get; set; }

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

    public List<Type> PrecedingMiddleware
    {
        get
        {
            _precedingMiddleware ??= new();
            return _precedingMiddleware;
        }
        set => _precedingMiddleware = value ?? throw new ArgumentNullException(nameof(value));
    }
    private List<Type>? _precedingMiddleware;

    public List<Type> FollowingMiddleware
    {
        get
        {
            _followingMiddleware ??= new();
            return _followingMiddleware;
        }
        set => _followingMiddleware = value ?? throw new ArgumentNullException(nameof(value));
    }
    private List<Type>? _followingMiddleware;

    public TimeSpan? Timeout { get; set; }
    public Func<Exception, Task>? ErrorHandler { get; set; }
    internal Action<MiddlewareBindingDescriptor>? Callback { get; set; }
    public string? Environment { get; set; }

    internal MiddlewareBindingDescriptor(Type middlewareType)
    {
        MiddlewareType = middlewareType;
    }

    public override bool Equals(object? obj)
    {
        if (obj is MiddlewareBindingDescriptor other)
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
