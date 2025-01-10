// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections;
using FluentInjections.Internal.Constants;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Policy;

namespace FluentInjections.Internal.Configurators;

internal abstract class MiddlewareConfigurator<TDependencyBuilder, TBinding>
    : Configurator<TBinding, MiddlewareBindingDescriptor>, IMiddlewareConfigurator, IConfigurator
    where TDependencyBuilder : class, IApplicationBuilder
    where TBinding : IBinding
{
    protected ConflictResolutionMode _conflictResolution = ConflictResolutionMode.WarnAndReplace;
    protected object _middleware = default!;
    protected Type _middlewareType = default!;
    protected TDependencyBuilder _dependencyBuilder;
    protected readonly List<MiddlewareBinding> _bindings = new();
    protected readonly List<Type> _registeredMiddlewareTypes = new();

    protected IServiceProvider Provider { get; set; }
    public IApplicationBuilder Application => _dependencyBuilder;

    protected MiddlewareConfigurator(TDependencyBuilder builder, IServiceProvider provider, ILogger logger) : base(logger)
    {
        _dependencyBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    protected internal void ValidateBindingsInternal() => ValidateBindings();

    internal IReadOnlyList<MiddlewareBindingDescriptor> MiddlewareDescriptors => _descriptors.AsReadOnly();
    internal IReadOnlyList<MiddlewareBinding> Bindings => _bindings;

    public object Middleware => _middleware!;
    public Type MiddlewareType => _middlewareType;

    public MiddlewareBindingDescriptor? GetDescriptor<TMiddleware>(MiddlewareBindingDescriptor? descriptor = null) where TMiddleware : class
    {
        var middleware = typeof(TMiddleware);
        var predicate = descriptor is null ? (Func<MiddlewareBindingDescriptor, bool>)(d => d.MiddlewareType == middleware) :
            (d => d.MiddlewareType == middleware && d.Name == descriptor.Name);
        return _descriptors.FirstOrDefault(predicate);
    }

    public bool RemoveMiddleware<TMiddleware>(MiddlewareBindingDescriptor? descriptor = null) where TMiddleware : class
    {
        var middleware = typeof(TMiddleware);
        var binding = GetDescriptor<TMiddleware>(descriptor) as MiddlewareBinding;
        descriptor ??= binding?.Descriptor ?? throw new InvalidOperationException("The descriptor is null.");

        if (descriptor is not null || binding is not null)
        {
            if ((descriptor is null || _descriptors.Remove(descriptor)) &&
                (binding is null || _bindings.Remove(binding)))
            {
                Debug.WriteLine($"The middleware component of type {middleware.Name} was removed successfully.");
                return true;
            }
        }
        return false;
    }

    public IMiddlewareBinding<TMiddleware> UseMiddleware<TMiddleware>() where TMiddleware : class
    {
        var descriptor = new MiddlewareBindingDescriptor(typeof(TMiddleware), this);
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

    public void ConfigureAll(Action<MiddlewareBindingDescriptor> configure)
    {
        Guard.NotNull(configure, nameof(configure));

        foreach (var binding in _descriptors)
        {
            configure(binding);
        }
    }

    #region Validation
    protected internal override void ValidateBindings()
    {
        foreach (var descriptor in _descriptors)
        {
            Debug.WriteLine($"Descriptor: {descriptor.MiddlewareType.Name}, Name: {descriptor.Name}");
        }

        var groups = _descriptors.GroupBy(binding => new { binding.MiddlewareType, binding.Name });
        var duplicateGroups = groups.Where(group => group.Count() > 1);
        var duplicates = duplicateGroups.Select(group => group.Key);

        if (duplicates.Any())
        {
            StringBuilder sb = new();
            foreach (var duplicate in duplicates)
            {
                if (sb.Length == 0)
                {
                    sb.AppendLine("Duplicate middleware bindings found:");
                }

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
                    case ConflictResolutionMode.Ignore:
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
        var list = bindings.Skip(1);
        foreach (var binding in list)
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

        if (newDescriptor.Priority > existingDescriptor.Priority || existingDescriptor.Priority == DefaultValues.Priority)
        {
            existingDescriptor.Priority = newDescriptor.Priority;
        }

        if (newDescriptor.Group is not DefaultValues.Group || existingDescriptor.Group is null)
        {
            existingDescriptor.Group = newDescriptor.Group;
        }

        if (newDescriptor.RequiredEnvironment is not null)
        {
            existingDescriptor.RequiredEnvironment = newDescriptor.RequiredEnvironment;
        }

        if (newDescriptor.ExecutionPolicyFactory is not null)
        {
            existingDescriptor.ExecutionPolicyFactory = newDescriptor.ExecutionPolicyFactory;
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
    #endregion

    internal abstract IApplicationBuilder GetApplication();

    /// <summary>
    /// Registers a binding with the service collection.
    /// </summary>
    /// <param name="register">The action to register the binding.</param>
    /// <remarks>
    /// This method is used internally to register bindings that require additional configuration.
    /// </remarks>
    internal void Register(Action<MiddlewareBindingDescriptor, HttpContext> register)
    {
        ValidateBindings();
        List<MiddlewareBindingDescriptor> orderedDescriptors = OrderBindingDescriptors();

        orderedDescriptors.ForEach(d => Register(d, register));
    }

    #region Ordering Middleware Descriptors
    protected override List<MiddlewareBindingDescriptor> OrderBindingDescriptors()
    {
        var graph = new Dictionary<MiddlewareBindingDescriptor, List<MiddlewareBindingDescriptor>>();

        // Initialize graph with all descriptors
        foreach (var descriptor in _descriptors)
        {
            graph[descriptor] = new List<MiddlewareBindingDescriptor>();
        }

        // Build the graph based on dependencies, preceding, and following relationships
        foreach (var descriptor in _descriptors)
        {
            if (descriptor.Dependencies != null)
            {
                foreach (var dependencyType in descriptor.Dependencies)
                {
                    var dependency = FindMiddlewareBindingDescriptor(dependencyType);
                    if (dependency != null)
                    {
                        graph[dependency].Add(descriptor); // Dependency must run before this middleware
                    }
                }
            }

            if (descriptor.PrecedingMiddleware != null)
            {
                foreach (var precedingType in descriptor.PrecedingMiddleware)
                {
                    var precedingMiddleware = FindMiddlewareBindingDescriptor(precedingType);
                    if (precedingMiddleware != null)
                    {
                        graph[precedingMiddleware].Add(descriptor); // Preceding middleware must run before this middleware
                    }
                }
            }

            if (descriptor.FollowingMiddleware != null)
            {
                foreach (var followingType in descriptor.FollowingMiddleware)
                {
                    var followingMiddleware = FindMiddlewareBindingDescriptor(followingType);
                    if (followingMiddleware != null)
                    {
                        graph[descriptor].Add(followingMiddleware); // This middleware must run before following middleware
                    }
                }
            }
        }

        // Perform topological sorting
        var sortedDescriptors = TopologicalSort(graph);

        // Apply secondary sorting by priority
        return sortedDescriptors
            .OrderBy(d => d.Priority)
            .ToList();
    }

    private MiddlewareBindingDescriptor? FindMiddlewareBindingDescriptor(Type middlewareType)
    {
        return _descriptors.FirstOrDefault(d => d.MiddlewareType == middlewareType);
    }

    private List<MiddlewareBindingDescriptor> TopologicalSort(Dictionary<MiddlewareBindingDescriptor, List<MiddlewareBindingDescriptor>> graph)
    {
        var sorted = new List<MiddlewareBindingDescriptor>();
        var visited = new HashSet<MiddlewareBindingDescriptor>();
        var visiting = new HashSet<MiddlewareBindingDescriptor>();

        foreach (var node in graph.Keys)
        {
            Visit(node, graph, sorted, visited, visiting);
        }

        return sorted;
    }

    private void Visit(
        MiddlewareBindingDescriptor node,
        Dictionary<MiddlewareBindingDescriptor, List<MiddlewareBindingDescriptor>> graph,
        List<MiddlewareBindingDescriptor> sorted,
        HashSet<MiddlewareBindingDescriptor> visited,
        HashSet<MiddlewareBindingDescriptor> visiting)
    {
        if (visited.Contains(node))
            return;

        if (visiting.Contains(node))
        {
            throw new InvalidOperationException($"Circular dependency detected with middleware: {node.MiddlewareType?.FullName}");
        }

        visiting.Add(node);

        foreach (var dependency in graph[node])
        {
            Visit(dependency, graph, sorted, visited, visiting);
        }

        visiting.Remove(node);
        visited.Add(node);
        sorted.Add(node);
    }
    #endregion

    #region Middleware Registration
    /// <summary>
    /// Registers a binding with the service collection.
    /// </summary>
    /// <param name="descriptor">The binding descriptor to register.</param>
    /// <param name="register">The action to register the binding.</param>
    /// <remarks>
    /// This method is used internally to register bindings that require additional configuration.
    /// </remarks>
    internal void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext>? register)
    {
        Guard.NotNull(descriptor, nameof(descriptor));

        var application = GetApplication();

        if (descriptor.IsEnabled && (descriptor.Condition is null || descriptor.Condition.Invoke()))
        {
            _registeredMiddlewareTypes.Add(descriptor.MiddlewareType);

            application.Use(async (HttpContext context, Func<Task> next) =>
            {
                try
                {
                    if (descriptor.Timeout.HasValue)
                    {
                        var timeoutToken = new CancellationTokenSource(descriptor.Timeout.Value).Token;
                        await Task.Run(async () =>
                        {
                            if (register is not null)
                            {
                                register(descriptor, context);
                                await next();
                            }
                            else
                            {
                                await InvokeMiddlewareWithFallbackAndPolicy(descriptor, context, next);
                            }
                        }, timeoutToken);
                    }
                    else
                    {
                        if (register is not null)
                        {
                            register(descriptor, context);
                            await next();
                        }
                        else
                        {
                            await InvokeMiddlewareWithFallbackAndPolicy(descriptor, context, next);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (descriptor.ErrorHandler != null)
                    {
                        await descriptor.ErrorHandler.Invoke(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
            });

            Debug.WriteLine($"Registered middleware: {descriptor.MiddlewareType.FullName}, Priority: {descriptor.Priority}");
        }
        else
        {
            Debug.WriteLine($"Skipped middleware: {descriptor.MiddlewareType.FullName}, Priority: {descriptor.Priority}");
        }
    }

    private async Task InvokeMiddlewareWithFallbackAndPolicy(MiddlewareBindingDescriptor descriptor, HttpContext context, Func<Task> next)
    {
        try
        {
            if (descriptor.ExecutionPolicyFactory is null)
            {
                await InvokeMiddleware(descriptor, context, next);
            }
            else
            {
                var policy = descriptor.ExecutionPolicyFactory(Provider) as IExecutionPolicy
                    ?? throw new InvalidOperationException("Execution policy factory returned null.");

                descriptor.ExecutionPolicyConfiguration?.Invoke(policy);

                await policy.ExecuteAsync(async () =>
                {
                    await InvokeMiddleware(descriptor, context, next);
                });
            }
        }
        catch (Exception ex)
        {
            if (descriptor.ErrorHandler is null)
            {
                throw;
            }

            await descriptor.ErrorHandler.Invoke(ex);

            if (descriptor.Fallback is not null)
            {
                await descriptor.Fallback.Invoke(context);
            }
        }
    }

    protected async Task InvokeMiddleware(MiddlewareBindingDescriptor descriptor, HttpContext context, Func<Task> next)
    {
        object? middlewareInstance = null;
        MethodInfo? method = null;
        object[]? args = null;

        try
        {
            var services = context.RequestServices;
            Debug.WriteLine($"Request Services type: {services.GetType().FullName}");
            middlewareInstance = context.RequestServices.GetRequiredService(descriptor.MiddlewareType);
            (method, args) = await GetInvokeMethod(middlewareInstance, context, next);
        }
        catch
        {
            if (descriptor.Fallback is not null)
            {
                await descriptor.Fallback.Invoke(context);
            }
            else
            {
                throw;
            }
        }

        method?.Invoke(middlewareInstance, args);
    }

    protected async Task InvokeMiddleware(object middleware, HttpContext context, Func<Task> next)
    {
        var (method, args) = await GetInvokeMethod(middleware, context, next);
        method.Invoke(middleware, args);
    }

    protected Task<(MethodInfo, object[])> GetInvokeMethod(object middleware, HttpContext context, Func<Task> next)
    {
        var method = middleware.GetType().GetMethod("InvokeAsync") ?? middleware.GetType().GetMethod("Invoke");

        if (method is null)
        {
            throw new InvalidOperationException($"Middleware {middleware.GetType().FullName} does not have an Invoke or InvokeAsync method.");
        }

        var parameters = method.GetParameters();
        var args = parameters.Length == 2
            ? new object[] { context, next }
            : new object[] { context };

        return Task.FromResult((method, args));
    }
    #endregion

    #region Middleware Binding
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

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithName(string name)
        {
            Guard.NotNullOrWhiteSpace(name, nameof(name));
            Descriptor.Name = name;
            Debug.WriteLine($"Named the middleware of type {Descriptor.MiddlewareType.Name} component {name}.");
            return this;
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
            Descriptor.Condition = () => func(default!);
            Debug.WriteLine($"Set the condition for the middleware component to {func.Method.Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithExecutionPolicy<TPolicy>(Action<TPolicy> configure) where TPolicy : class
        {
            Guard.NotNull(configure, nameof(configure));
            Descriptor.ExecutionPolicyConfiguration = policy => configure((TPolicy)policy);
            Debug.WriteLine("Set the execution policy for the middleware component.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithExecutionPolicy<TPolicy>(Func<IServiceProvider, TPolicy> factory) where TPolicy : class
        {
            Guard.NotNull(factory, nameof(factory));
            Descriptor.ExecutionPolicyFactory = provider => factory(provider);
            Debug.WriteLine("Set the execution policy factory for the middleware component.");
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareBinding<TMiddleware> WithExecutionPolicy<TPolicy>(TPolicy policy) where TPolicy : class
        {
            Descriptor.ExecutionPolicyFactory = provider => policy;
            Debug.WriteLine("Set the execution policy for the middleware component.");
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

            Guard.NotNull(middleware, nameof(instance));

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
    #endregion
}
