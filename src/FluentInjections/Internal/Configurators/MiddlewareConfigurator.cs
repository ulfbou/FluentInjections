// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

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
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> Disable()
        {
            Descriptor.IsEnabled = false;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> Enable()
        {
            Descriptor.IsEnabled = true;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> Follows<TFollowingMiddleware>()
        {
            Descriptor.FollowingMiddleware.Add(typeof(TFollowingMiddleware));
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> InGroup(string group)
        {
            Guard.NotNullOrWhiteSpace(group, nameof(group));

            Descriptor.Group = group;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> OnError(Func<Exception, Task> errorHandler)
        {
            Guard.NotNull(errorHandler, nameof(errorHandler));

            Descriptor.ErrorHandler = errorHandler;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> Precedes<TPrecedingMiddleware>()
        {
            Descriptor.PrecedingMiddleware.Add(typeof(TPrecedingMiddleware));
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> RequireEnvironment(string environment)
        {
            Guard.NotNullOrWhiteSpace(environment, nameof(environment));

            Descriptor.Environment = environment;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> When(Func<bool> func)
        {
            Guard.NotNull(func, nameof(func));

            Descriptor.Condition = func;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> When<TContext>(Func<TContext, bool> func)
        {
            Guard.NotNull(func, nameof(func));

            // TODO: Handle context-based conditions correctly. 
            Descriptor.Condition = () => func(default!);
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithExecutionPolicy<TPolicy>(Action<TPolicy> value) where TPolicy : class
        {
            Guard.NotNull(value, nameof(value));

            Descriptor.ExecutionPolicy = value;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithFallback(Func<object, Task> fallback)
        {
            Guard.NotNull(fallback, nameof(fallback));

            Descriptor.Fallback = fallback;
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
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithMetadata<TMetadata>(TMetadata metadata)
        {
            Guard.NotNull(metadata, nameof(metadata));

            Descriptor.Metadata = metadata;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithOptions<TOptions>(TOptions options) where TOptions : class
        {
            Guard.NotNull(options, nameof(options));

            Descriptor.Options = options;
            Descriptor.OptionsType = typeof(TOptions);
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithPriority(int priority)
        {
            Descriptor.Priority = priority;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithPriority(Func<int> priority)
        {
            Guard.NotNull(priority, nameof(priority));

            Descriptor.Priority = priority();
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithPriority<TContext>(Func<TContext, int> priority)
        {
            Guard.NotNull(priority, nameof(priority));

            // TODO: Handle context-based priorities correctly.
            var context = default(TContext);
            Descriptor.Priority = priority(context!);
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithTag(string tag)
        {
            Guard.NotNullOrWhiteSpace(tag, nameof(tag));

            Descriptor.Tag = tag;
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithTimeout(TimeSpan timeout)
        {
            Descriptor.Timeout = timeout;
            return this;
        }
    }
}
