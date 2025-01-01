// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Core;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using System.Diagnostics;

namespace FluentInjections.Internal.Configurators;

public abstract class MiddlewareConfigurator<TBuilder> : IMiddlewareConfigurator
{
    private object? _middleware;
    private Type? _middlewareType;
    private List<MiddlewareBindingDescriptor> _descriptors = new();
    private List<MiddlewareBinding> _bindings = new();

    internal IReadOnlyList<MiddlewareBindingDescriptor> MiddlewareBindings => _descriptors.AsReadOnly();

    public object Middleware => _middleware!;
    public Type MiddlewareType => _middlewareType!;
    public ConflictResolutionMode ConflictResolution { get; set; }

    public MiddlewareConfigurator()
    {
        ConflictResolution = ConflictResolutionMode.WarnAndReplace;
    }

    /// <inheritdoc/>
    public IMiddlewareBinding<TMiddleware>? GetMiddleware<TMiddleware>(MiddlewareBindingDescriptor? descriptor = null) where TMiddleware : class
    {
        var middleware = typeof(TMiddleware);
        var predicate = descriptor is not null
            ? (Func<MiddlewareBinding, bool>)(b => b.Descriptor.Equals(descriptor))
            : b => b.Descriptor.MiddlewareType == middleware;

        return _bindings.FirstOrDefault(predicate) as IMiddlewareBinding<TMiddleware>;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public IMiddlewareBinding<TMiddleware> UseMiddleware<TMiddleware>() where TMiddleware : class
    {
        _middleware = null;
        _middlewareType = typeof(TMiddleware);
        var descriptor = new MiddlewareBindingDescriptor(_middlewareType);
        var binding = new MiddlewareBinding<TMiddleware>(descriptor);
        _descriptors.Add(descriptor);
        _bindings.Add(binding);
        return binding;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void ConfigureAll(Action<IMiddlewareBinding> configure)
    {
        Guard.NotNull(configure, nameof(configure));

        foreach (var binding in _bindings)
        {
            configure(binding);
        }
    }

    /// <inheritdoc/>
    public void Register() => _descriptors.ForEach(d => Register(d));

    internal void Register(Action<MiddlewareBindingDescriptor, HttpContext, TBuilder> register) => _descriptors.ForEach(d => Register(d, register));

    // TODO: Implement the registration of middleware components correctly.
    protected abstract void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext, TBuilder>? register = null);

    /// <inheritdoc/>
    internal class MiddlewareBinding : IMiddlewareBinding
    {
        public MiddlewareBindingDescriptor Descriptor { get; }

        public MiddlewareBinding(MiddlewareBindingDescriptor descriptor)
        {
            Descriptor = descriptor;
        }
    }

    /// <inheritdoc/>
    internal class MiddlewareBinding<TMiddleware> : MiddlewareBinding, IMiddlewareBinding<TMiddleware> where TMiddleware : class
    {
        public TMiddleware Instance { get; private set; }

        public MiddlewareBinding(MiddlewareBindingDescriptor descriptor, TMiddleware? instance = default)
            : base(descriptor)
        {
            Instance = instance!;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> DependsOn<TOtherMiddleware>()
        {
            Descriptor.Dependencies.Add(typeof(TOtherMiddleware));
            Debug.WriteLine($"Set the middleware component to depend on {typeof(TOtherMiddleware).Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> Follows<TFollowingMiddleware>()
        {
            Descriptor.FollowingMiddleware.Add(typeof(TFollowingMiddleware));
            Debug.WriteLine($"Set the middleware component to follow {typeof(TFollowingMiddleware).Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> InGroup(string group)
        {
            Guard.NotNullOrWhiteSpace(group, nameof(group));

            Descriptor.Group = group;
            Debug.WriteLine($"Grouped the middleware component with {group}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> OnError(Func<Exception, Task> errorHandler)
        {
            Guard.NotNull(errorHandler, nameof(errorHandler));

            Descriptor.ErrorHandler = errorHandler;
            Debug.WriteLine("Set the error handler for the middleware component.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> Precedes<TPrecedingMiddleware>()
        {
            Descriptor.PrecedingMiddleware.Add(typeof(TPrecedingMiddleware));
            Debug.WriteLine($"Set the middleware component to precede {typeof(TPrecedingMiddleware).Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> When(Func<bool> func)
        {
            Guard.NotNull(func, nameof(func));

            Descriptor.Condition = func;
            Debug.WriteLine($"Set the condition for the middleware component to {func.Method.Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> When<TContext>(Func<TContext, bool> func)
        {
            Guard.NotNull(func, nameof(func));

            // TODO: Handle context-based conditions correctly. 
            Descriptor.Condition = () => func(default!);
            Debug.WriteLine($"Set the condition for the middleware component to {func.Method.Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithExecutionPolicy<TPolicy>(Action<TPolicy> value) where TPolicy : class
        {
            Guard.NotNull(value, nameof(value));

            Descriptor.ExecutionPolicy = value;
            Debug.WriteLine($"Set the execution policy for the middleware component to {typeof(TPolicy).Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithFallback(Func<object, Task> fallback)
        {
            Guard.NotNull(fallback, nameof(fallback));

            Descriptor.Fallback = fallback;
            Debug.WriteLine("Set the fallback function for the middleware component.");
            return this;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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


        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithOptions<TOptions>(TOptions options) where TOptions : class
        {
            Guard.NotNull(options, nameof(options));

            Descriptor.Options = options;
            Descriptor.OptionsType = typeof(TOptions);
            Debug.WriteLine($"Added options of type {typeof(TOptions).Name} to the middleware component.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithPriority(int priority)
        {
            Descriptor.Priority = priority;
            Debug.WriteLine($"Set the priority of the middleware component to {priority}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithPriority(Func<int> priority)
        {
            Guard.NotNull(priority, nameof(priority));

            Descriptor.Priority = priority();
            Debug.WriteLine($"Set the priority of the middleware component to {Descriptor.Priority}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithPriority<TContext>(Func<TContext, int> priority)
        {
            Guard.NotNull(priority, nameof(priority));

            // TODO: Handle context-based priorities correctly.
            var context = default(TContext);
            Descriptor.Priority = priority(context!);
            Debug.WriteLine($"Set the priority of the middleware component to {Descriptor.Priority}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithTag(string tag)
        {
            Guard.NotNullOrWhiteSpace(tag, nameof(tag));

            Descriptor.Tag = tag;
            Debug.WriteLine($"Tagged the middleware component with {tag}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithTimeout(TimeSpan timeout)
        {
            Descriptor.Timeout = timeout;
            Debug.WriteLine($"Set the timeout of the middleware component to {timeout}.");
            return this;
        }
    }
}
