using FluentInjections.Constants;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Internal.Configurators;

internal class MiddlewareDescriptor
{
    internal Type MiddlewareType { get; set; }

    internal int Priority
    {
        get => _priority ?? DefaultValues.Priority;
        set => _priority = value;
    }
    private int? _priority;

    internal string Group
    {
        get => _group ?? DefaultValues.Group;
        set => _group = value ?? throw new ArgumentNullException(nameof(value));
    }
    private string? _group;

    internal string RequiredEnvironment
    {
        get => _requiredEnvironment ?? DefaultValues.Environment;
        set => _requiredEnvironment = value ?? throw new ArgumentNullException(nameof(value));
    }
    private string? _requiredEnvironment;

    internal object? ExecutionPolicy { get; set; }
    internal Func<object, Task>? Fallback { get; set; }
    internal object? Options { get; set; }
    internal object? Metadata { get; set; }
    internal string? Tag { get; set; }
    internal Func<bool>? Condition { get; set; }

    internal bool IsEnabled
    {
        get => _isEnabled ?? true;
        set => _isEnabled = value;
    }
    private bool? _isEnabled;

    internal List<Type> Dependencies
    {
        get => _dependencies;
        set => _dependencies = value ?? throw new ArgumentNullException(nameof(value));
    }
    private List<Type> _dependencies = new();

    internal List<Type>? PrecedingMiddleware
    {
        get => _precedingMiddleware;
        set => _precedingMiddleware = value;
    }
    private List<Type>? _precedingMiddleware = new();

    internal List<Type>? FollowingMiddleware
    {
        get => _followingMiddleware;
        set => _followingMiddleware = value;
    }
    private List<Type>? _followingMiddleware = new();

    internal TimeSpan? Timeout { get; set; }
    internal Func<Exception, Task>? ErrorHandler { get; set; }

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
