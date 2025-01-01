// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Core;

using FluentInjections.Internal.Constants;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FluentInjections.Internal.Configurators;

internal abstract class MiddlewareConfigurator<TBuilder> : IMiddlewareConfigurator
{
    protected readonly List<MiddlewareBindingDescriptor> _descriptors = new();
    private readonly List<MiddlewareBinding> _bindings = new();
    protected readonly ILogger _logger;
    protected ConflictResolutionMode _conflictResolution = ConflictResolutionMode.WarnAndReplace;
    protected object _middleware = default!;
    protected Type _middlewareType = default!;

    public MiddlewareConfigurator(ILogger? logger = null)
    {
        _logger = logger ?? LoggerUtility.CreateLogger<MiddlewareConfigurator<TBuilder>>();
    }

    internal IReadOnlyList<MiddlewareBindingDescriptor> MiddlewareBindings => _descriptors.AsReadOnly();

    public ConflictResolutionMode ConflictResolution
    {
        get => _conflictResolution;
        set
        {
            // Validate the conflict resolution mode.
            if (!Enum.IsDefined(typeof(ConflictResolutionMode), value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Invalid conflict resolution mode.");
            }

            _conflictResolution = value;
        }
    }

    public object Middleware => _middleware!;
    public Type MiddlewareType => _middlewareType;

    public IMiddlewareBinding<TMiddleware>? GetMiddleware<TMiddleware>(MiddlewareBindingDescriptor? descriptor = null) where TMiddleware : class
    {
        var middleware = typeof(TMiddleware);
        var predicate = descriptor is not null
            ? (Func<MiddlewareBinding, bool>)(b => b.Descriptor.Equals(descriptor))
            : b => b.Descriptor.MiddlewareType == middleware;
        return _bindings.FirstOrDefault(predicate) as IMiddlewareBinding<TMiddleware>;
    }

    public bool RemoveMiddleware<TMiddleware>(MiddlewareBindingDescriptor? descriptor = null) where TMiddleware : class
    {
        var middleware = typeof(TMiddleware);
        var binding = GetMiddleware<TMiddleware>(descriptor) as MiddlewareBinding;
        descriptor ??= binding?.Descriptor ?? throw new InvalidOperationException("The descriptor is null.");
        if (descriptor is not null || binding is not null)
        {
            if ((descriptor is null || _descriptors.Remove(descriptor)) &&
                (binding is null || _bindings.Remove((binding as MiddlewareBinding)!)))
            {
                Debug.WriteLine($"The middleware component of type {middleware.Name} was removed successfully.");
                return true;
            }
        }
        return false;
    }

    public IMiddlewareBinding<TMiddleware> UseMiddleware<TMiddleware>() where TMiddleware : class
    {
        var descriptor = new MiddlewareBindingDescriptor(typeof(TMiddleware));
        var binding = new MiddlewareBinding<TMiddleware>(descriptor);
        _descriptors.Add(descriptor);
        _bindings.Add(binding);
        return binding;
    }

    public void ApplyGroupPolicy(string groupName, Action<IMiddlewareBinding> configure)
    {
        Guard.NotNullOrWhiteSpace(groupName, nameof(groupName));
        Guard.NotNull(configure, nameof(configure));
        var bindings = _bindings.Where(b => b.Descriptor.Group == groupName);
        foreach (var binding in bindings)
        {
            configure(binding);
        }
    }

    public void ConfigureAll(Action<IMiddlewareBinding> configure)
    {
        Guard.NotNull(configure, nameof(configure));
        foreach (var binding in _bindings)
        {
            configure(binding);
        }
    }

    public void Register()
    {
        ValidateBindings();
        _descriptors.ForEach(d => Register(d));
    }

    private void ValidateBindings()
    {
        var duplicates = _descriptors.GroupBy(binding => new { binding.MiddlewareType, binding.Name })
                                     .Where(group => group.Count() > 1)
                                     .Select(group => group.Key);

        if (duplicates.Any())
        {
            StringBuilder sb = new();
            sb.AppendLine("Duplicate middleware bindings found:");

            foreach (var duplicate in duplicates)
            {
                var message = $"Duplicate middleware binding for type {duplicate.MiddlewareType.Name}";

                if (duplicate.Name is not null)
                {
                    message += $" with name {duplicate.Name}";
                }

                switch (ConflictResolution)
                {
                    case ConflictResolutionMode.WarnAndReplace:
                        _logger.LogWarning(message);
                        ReplaceBinding(duplicate);
                        break;
                    case ConflictResolutionMode.Replace:
                        ReplaceBinding(duplicate);
                        break;
                    case ConflictResolutionMode.Prevent:
                        sb.AppendLine(message);
                        break;
                    case ConflictResolutionMode.Merge:
                        _logger.LogWarning(message);
                        MergeBinding(duplicate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid conflict resolution mode.");
                }

                sb.AppendLine(message);
            }

            if (ConflictResolution == ConflictResolutionMode.Prevent)
            {
                throw new InvalidOperationException(sb.ToString());
            }
        }
    }

    private void ReplaceBinding(dynamic duplicate)
    {
        var bindings = _descriptors.Where(binding => binding.MiddlewareType == duplicate.MiddlewareType && binding.Name == duplicate.Name).ToList();
        foreach (var binding in bindings.Skip(1))
        {
            _descriptors.Remove(binding);
        }
    }

    private void MergeBinding(dynamic duplicate)
    {
        var bindings = _descriptors.Where(binding => binding.MiddlewareType == duplicate.MiddlewareType && binding.Name == duplicate.Name).ToList();
        var primaryBinding = bindings.First();

        foreach (var binding in bindings.Skip(1))
        {
            MergeDescriptors(primaryBinding, binding);
            _descriptors.Remove(binding);
        }
    }

    private void MergeDescriptors(MiddlewareBindingDescriptor existingDescriptor, MiddlewareBindingDescriptor newDescriptor)
    {
        if (newDescriptor.Instance is not null)
        {
            existingDescriptor.Instance = newDescriptor.Instance;
        }

        if (newDescriptor.Priority != DefaultValues.Priority)
        {
            existingDescriptor.Priority = newDescriptor.Priority;
        }

        if (newDescriptor.Group is not DefaultValues.Group)
        {
            existingDescriptor.Group = newDescriptor.Group;
        }

        if (newDescriptor.RequiredEnvironment is not null)
        {
            existingDescriptor.RequiredEnvironment = newDescriptor.RequiredEnvironment;
        }

        if (newDescriptor.ExecutionPolicy is not null)
        {
            existingDescriptor.ExecutionPolicy = newDescriptor.ExecutionPolicy;
        }

        if (newDescriptor.Fallback is not null)
        {
            existingDescriptor.Fallback = newDescriptor.Fallback;
        }

        if (newDescriptor.Options is not null)
        {
            existingDescriptor.Options = newDescriptor.Options;
        }

        if (newDescriptor.OptionsType is not null)
        {
            existingDescriptor.OptionsType = newDescriptor.OptionsType;
        }

        var dependencies = new HashSet<Type>(newDescriptor.Dependencies);
        foreach (var dependency in dependencies)
        {
            existingDescriptor.Dependencies.Add(dependency);

        }

        var precedingMiddleware = new HashSet<Type>(newDescriptor.PrecedingMiddleware);
        foreach (var middleware in precedingMiddleware)
        {
            existingDescriptor.PrecedingMiddleware.Add(middleware);
        }

        var followingMiddleware = new HashSet<Type>(newDescriptor.FollowingMiddleware);
        foreach (var middleware in followingMiddleware)
        {
            existingDescriptor.FollowingMiddleware.Add(middleware);
        }

        if (newDescriptor.Timeout is not null)
        {
            existingDescriptor.Timeout = newDescriptor.Timeout;
        }

        if (newDescriptor.Tag is not null)
        {
            existingDescriptor.Tag = newDescriptor.Tag;
        }

        if (newDescriptor.Condition is not null)
        {
            existingDescriptor.Condition += newDescriptor.Condition;
        }

        if (newDescriptor.Options is not null)
        {
            existingDescriptor.Options = newDescriptor.Options;
        }

        if (newDescriptor.OptionsType is not null)
        {
            existingDescriptor.OptionsType = newDescriptor.OptionsType;
        }

        if (newDescriptor.Metadata.Any())
        {
            foreach (var metadata in newDescriptor.Metadata)
            {
                existingDescriptor.Metadata[metadata.Key] = metadata.Value;
            }
        }

        if (newDescriptor.ErrorHandler is not null)
        {
            existingDescriptor.ErrorHandler += newDescriptor.ErrorHandler;
        }
    }

    protected abstract void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext, TBuilder>? register = null);

    internal class MiddlewareBinding : IMiddlewareBinding
    {
        public MiddlewareBindingDescriptor Descriptor { get; }

        public MiddlewareBinding(MiddlewareBindingDescriptor descriptor)
        {
            Descriptor = descriptor;
        }
    }

    internal class MiddlewareBinding<TMiddleware> : MiddlewareBinding, IMiddlewareBinding<TMiddleware> where TMiddleware : class
    {
        public TMiddleware Instance { get; private set; }

        public MiddlewareBinding(MiddlewareBindingDescriptor descriptor, TMiddleware? instance = default)
            : base(descriptor)
        {
            Instance = instance!;
        }

        public IMiddlewareBinding<TMiddleware> DependsOn<TOtherMiddleware>()
        {
            Descriptor.Dependencies.Add(typeof(TOtherMiddleware));
            Debug.WriteLine($"Set the middleware component to depend on {typeof(TOtherMiddleware).Name}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> Follows<TFollowingMiddleware>()
        {
            Descriptor.FollowingMiddleware.Add(typeof(TFollowingMiddleware));
            Debug.WriteLine($"Set the middleware component to follow {typeof(TFollowingMiddleware).Name}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> InGroup(string group)
        {
            Guard.NotNullOrWhiteSpace(group, nameof(group));
            Descriptor.Group = group;
            Debug.WriteLine($"Grouped the middleware component with {group}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> OnError(Func<Exception, Task> errorHandler)
        {
            Guard.NotNull(errorHandler, nameof(errorHandler));
            Descriptor.ErrorHandler = errorHandler;
            Debug.WriteLine("Set the error handler for the middleware component.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> Precedes<TPrecedingMiddleware>()
        {
            Descriptor.PrecedingMiddleware.Add(typeof(TPrecedingMiddleware));
            Debug.WriteLine($"Set the middleware component to precede {typeof(TPrecedingMiddleware).Name}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> When(Func<bool> func)
        {
            Guard.NotNull(func, nameof(func));
            Descriptor.Condition = func;
            Debug.WriteLine($"Set the condition for the middleware component to {func.Method.Name}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> When<TContext>(Func<TContext, bool> func)
        {
            Guard.NotNull(func, nameof(func));
            Descriptor.Condition = () => func(default!);
            Debug.WriteLine($"Set the condition for the middleware component to {func.Method.Name}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithExecutionPolicy<TPolicy>(Action<TPolicy> value) where TPolicy : class
        {
            Guard.NotNull(value, nameof(value));
            Descriptor.ExecutionPolicy = value;
            Debug.WriteLine($"Set the execution policy for the middleware component to {typeof(TPolicy).Name}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithFallback(Func<object, Task> fallback)
        {
            Guard.NotNull(fallback, nameof(fallback));
            Descriptor.Fallback = fallback;
            Debug.WriteLine("Set the fallback function for the middleware component.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithInstance(object instance)
        {
            var middleware = instance as TMiddleware;
            if (middleware == null)
            {
                throw new ArgumentException($"The instance must be of type {typeof(TMiddleware).Name}.", nameof(instance));
            }
            Instance = middleware;
            Debug.WriteLine($"Set the instance of the middleware component to {middleware.GetType().Name}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithMetadata(string name, object value)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(value, nameof(value));
            if (Descriptor.Metadata.ContainsKey(name))
            {
                throw new InvalidOperationException($"Metadata with name {name} already exists.");
            }
            Descriptor.Metadata.Add(name, value);
            Debug.WriteLine($"Added metadata with name {name} to the middleware component.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithOptions<TOptions>(TOptions options) where TOptions : class
        {
            Guard.NotNull(options, nameof(options));
            Descriptor.Options = options;
            Descriptor.OptionsType = typeof(TOptions);
            Debug.WriteLine($"Added options of type {typeof(TOptions).Name} to the middleware component.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithPriority(int priority)
        {
            Descriptor.Priority = priority;
            Debug.WriteLine($"Set the priority of the middleware component to {priority}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithPriority(Func<int> priority)
        {
            Guard.NotNull(priority, nameof(priority));
            Descriptor.Priority = priority();
            Debug.WriteLine($"Set the priority of the middleware component to {Descriptor.Priority}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithPriority<TContext>(Func<TContext, int> priority)
        {
            Guard.NotNull(priority, nameof(priority));
            var context = default(TContext);
            Descriptor.Priority = priority(context!);
            Debug.WriteLine($"Set the priority of the middleware component to {Descriptor.Priority}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithTag(string tag)
        {
            Guard.NotNullOrWhiteSpace(tag, nameof(tag));
            Descriptor.Tag = tag;
            Debug.WriteLine($"Tagged the middleware component with {tag}.");
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithTimeout(TimeSpan timeout)
        {
            Descriptor.Timeout = timeout;
            Debug.WriteLine($"Set the timeout of the middleware component to {timeout}.");
            return this;
        }
    }
}
