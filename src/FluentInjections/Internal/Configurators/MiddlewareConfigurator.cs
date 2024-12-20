using System.Diagnostics;
using System.Net.Http;

using Autofac;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using static System.Net.Mime.MediaTypeNames;

namespace FluentInjections.Internal.Configurators;

internal class MiddlewareConfigurator<TApplication> : IMiddlewareConfigurator<TApplication> where TApplication : class
{
    private readonly IServiceCollection _services;

    private readonly TApplication _application;
    private readonly List<MiddlewareDescriptor> _middlewares = new();
    private readonly CancellationTokenSource _cts = new();

    public TApplication Application => _application;

    public MiddlewareConfigurator(IServiceCollection services, TApplication application)
    {
        _services = services;
        _application = application;
    }

    public IMiddlewareBinding UseMiddleware<TMiddleware>() where TMiddleware : class => UseMiddleware(typeof(TMiddleware));

    public IMiddlewareBinding UseMiddleware(Type middlewareType)
    {
        ArgumentGuard.NotNull(middlewareType, nameof(middlewareType));

        var descriptor = new MiddlewareDescriptor(middlewareType);

        _middlewares.Add(descriptor);
        Debug.WriteLine($"Registered middleware: {descriptor.MiddlewareType.FullName}");
        return new MiddlewareBinding(_services, _application, descriptor);
    }

    public IMiddlewareBinding RemoveMiddleware<TMiddleware>() where TMiddleware : class => RemoveMiddleware(typeof(TMiddleware));

    public IMiddlewareBinding RemoveMiddleware(Type middlewareType)
    {
        ArgumentGuard.NotNull(middlewareType, nameof(middlewareType));

        var descriptor = new MiddlewareDescriptor(middlewareType);

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Middleware of type {middlewareType.Name} is not registered.");
        }

        _middlewares.Remove(descriptor);
        Debug.WriteLine($"Removed middleware: {descriptor.MiddlewareType.FullName}");

        return new MiddlewareBinding(_services, _application, descriptor);
    }

    public IMiddlewareBinding GetMiddleware<TMiddleware>() where TMiddleware : class => GetMiddleware(typeof(TMiddleware));

    public IMiddlewareBinding GetMiddleware(Type middlewareType)
    {
        ArgumentGuard.NotNull(middlewareType, nameof(middlewareType));

        var descriptor = _middlewares.FirstOrDefault(m => m.MiddlewareType == middlewareType);

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Middleware of type {middlewareType.Name} is not registered.");
        }

        return new MiddlewareBinding(_services, _application, descriptor);
    }

    public void ApplyGroupPolicy(string groupName, Action<IMiddlewareBinding> configure)
    {
        ArgumentGuard.NotNullOrEmpty(groupName, nameof(groupName));
        ArgumentGuard.NotNull(configure, nameof(configure));

        var groupMiddlewares = _middlewares.Where(m => m.Group == groupName);

        foreach (var descriptor in groupMiddlewares)
        {
            configure(new MiddlewareBinding(_services, _application, descriptor));
        }

        Debug.WriteLine($"Applied group policy to middleware group: {groupName}. Configured {groupMiddlewares.Count()} middlewares.");
    }

    public void ConfigureAll(Action<IMiddlewareBinding> configure)
    {
        ArgumentGuard.NotNull(configure, nameof(configure));

        var allMiddlewares = _middlewares;

        foreach (var descriptor in allMiddlewares)
        {
            configure(new MiddlewareBinding(_services, _application, descriptor));
        }

        Debug.WriteLine($"Configured all middlewares. Total: {allMiddlewares.Count}");
    }

    public void Register()
    {
        IApplicationBuilder application = (_application as IApplicationBuilder)!;

        // Order middleware descriptors
        var orderedMiddlewares = OrderMiddlewareDescriptors(_middlewares);

        // Add middleware to the pipeline in the correct order
        foreach (var descriptor in orderedMiddlewares)
        {
            RegisterMiddleware(descriptor, application);
        }

        Debug.WriteLine("Registered all middleware.");
    }

    #region Register helpers
    // Register with callback to allow for custom testing setup. 
    internal void Register(Action<MiddlewareDescriptor, HttpContext, IApplicationBuilder> registerMiddlewareCallback)
    {
        IApplicationBuilder application = (_application as IApplicationBuilder)!;

        // Order middleware descriptors
        var orderedMiddlewares = OrderMiddlewareDescriptors(_middlewares);

        // Add middleware to the pipeline in the correct order
        foreach (var descriptor in orderedMiddlewares)
        {
            RegisterMiddleware(descriptor, application, registerMiddlewareCallback);
        }

        Debug.WriteLine($"Registered all middleware ({orderedMiddlewares.Count()}) with test method callbacks.");
    }

    private void RegisterMiddleware(
        MiddlewareDescriptor descriptor, IApplicationBuilder application,
        Action<MiddlewareDescriptor, HttpContext, IApplicationBuilder>? callback = null)
    {
        application.Use(async (HttpContext context, RequestDelegate next) =>
        {
            if (CheckActivationConditions(descriptor, application, context, next))
            {
                if (descriptor.Timeout is not null)
                {
                    context.RequestAborted.Register(() => _cts.CancelAfter(descriptor.Timeout.Value));
                    context.RequestAborted = _cts.Token;
                }

                if (!TryResolveService(descriptor, out var middlewareInstance))
                {
                    throw new InvalidOperationException($"Middleware {descriptor.MiddlewareType.FullName} could not be resolved.");
                }

                callback?.Invoke(descriptor, context, application);
                await InvokeMiddleware(middlewareInstance, descriptor, context, next);
            }

            await next(context);
        });

        Debug.WriteLine($"Registered middleware: {descriptor.MiddlewareType.FullName}, Priority: {descriptor.Priority}, Group: {descriptor.Group}");
    }

    private bool CheckActivationConditions(MiddlewareDescriptor descriptor, IApplicationBuilder application, HttpContext context, RequestDelegate next)
    {
        if (!descriptor.IsEnabled || descriptor.Condition?.Invoke() == true)
        {
            return false;
        }

        if (descriptor.RequiredEnvironment != null)
        {
            var env = application.ApplicationServices.GetRequiredService<IHostEnvironment>();

            if (env.EnvironmentName != descriptor.RequiredEnvironment)
            {
                return false;
            }
        }

        if (descriptor.ExecutionPolicy is not null)
        {
            var policy = descriptor.ExecutionPolicy;

            if (policy is IExecutionPolicy executionPolicy)
            {
                if (executionPolicy.CanExecute(context, next))
                {
                    return true;
                }
            }
            else
            {
                throw new InvalidOperationException($"Execution policy for middleware {descriptor.MiddlewareType.FullName} does not implement IExecutionPolicy.");
            }
        }
        else
        {
            return true;
        }

        return false;
    }

    private bool TryResolveService(MiddlewareDescriptor descriptor, out object middlewareInstance)
    {
        var serviceProvider = _services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var container = scope.ServiceProvider.GetRequiredService<ILifetimeScope>();

            try
            {
                if (descriptor.Options is not null)
                {
                    middlewareInstance = container.Resolve(descriptor.MiddlewareType, new TypedParameter(descriptor.Options.GetType(), descriptor.Options));
                }
                else
                {
                    middlewareInstance = container.Resolve(descriptor.MiddlewareType);
                }
                Debug.WriteLine($"Resolved middleware: {descriptor.MiddlewareType.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resolving middleware {descriptor.MiddlewareType.FullName}: {ex.Message}");
                middlewareInstance = null!;
                return false;
            }
        }
    }

    private static async Task InvokeMiddleware(object middleware, MiddlewareDescriptor descriptor, HttpContext context, RequestDelegate next)
    {
        // TODO: Cache the method info for performance
        var method = middleware.GetType().GetMethod("InvokeAsync") ?? middleware.GetType().GetMethod("Invoke");

        if (method is null)
        {
            throw new InvalidOperationException($"Middleware {middleware.GetType().FullName} does not have an Invoke or InvokeAsync method.");
        }

        var parameters = method.GetParameters();
        var args = parameters.Length == 2
            ? new object[] { context, next }
            : new object[] { context };

        if (descriptor.Timeout != null)
        {
            var timeout = descriptor.Timeout;
            var cts = new CancellationTokenSource(timeout.Value);
            context.RequestAborted.Register(() => cts.CancelAfter(timeout.Value));
            context.RequestAborted = cts.Token;
        }

        try
        {
            if (method.ReturnType == typeof(void))
            {
                method.Invoke(middleware, args);
            }
            else
            {
                await (Task)method.Invoke(middleware, args)!;
            }
        }
        catch (Exception ex)
        {
            if (descriptor.Fallback is not null)
            {
                await HandleFallbackAsync(descriptor.Fallback, middleware, descriptor.ErrorHandler);
            }
            else if (descriptor.ErrorHandler is not null)
            {
                await descriptor.ErrorHandler(ex);
            }
            else
            {
                throw;
            }
        }
    }

    private static async Task HandleFallbackAsync(Func<object, Task> fallback, object middleware, Func<Exception, Task>? errorHandler)
    {
        try
        {
            await fallback(middleware);
        }
        catch (Exception ex2)
        {
            if (errorHandler is not null)
            {
                await errorHandler(ex2);
            }
            else
            {
                throw;
            }
        }
    }
    #endregion

    #region Middleware Ordering
    private List<MiddlewareDescriptor> OrderMiddlewareDescriptors(List<MiddlewareDescriptor> descriptors)
    {
        // Step 1: Group middleware by Group property
        var groups = descriptors.GroupBy(m => m.Group)
                                .ToDictionary(g => g.Key, g => g.ToList());

        // Step 2: Sort middleware within each group
        var sortedGroups = new List<List<MiddlewareDescriptor>>();

        foreach (var group in groups.Values)
        {
            var groupGraph = BuildGraph(group);
            var sortedGroup = TopologicalSort(groupGraph);
            sortedGroups.Add(sortedGroup.OrderByDescending(m => m.Priority).ToList());
        }

        // Step 3: Determine group order
        var groupOrder = SortGroups(groups);

        // Step 4: Combine sorted groups in the correct order
        var sortedDescriptors = new List<MiddlewareDescriptor>();
        foreach (var groupName in groupOrder)
        {
            sortedDescriptors.AddRange(sortedGroups.First(g => g.First().Group == groupName));
        }

        Debug.WriteLine($"Ordered {sortedDescriptors.Count} middleware descriptors:");
        foreach (var descriptor in sortedDescriptors)
        {
            Debug.WriteLine($"Middleware: {descriptor.MiddlewareType.FullName}, Priority: {descriptor.Priority}, Group: {descriptor.Group}");
        }
        return sortedDescriptors;
    }

    private Dictionary<MiddlewareDescriptor, List<MiddlewareDescriptor>> BuildGraph(List<MiddlewareDescriptor> group)
    {
        var graph = new Dictionary<MiddlewareDescriptor, List<MiddlewareDescriptor>>();

        foreach (var descriptor in group)
        {
            if (!graph.ContainsKey(descriptor))
            {
                graph[descriptor] = new List<MiddlewareDescriptor>();
            }

            // Add dependencies
            foreach (var dependency in descriptor.Dependencies ?? Enumerable.Empty<Type>())
            {
                var dependencyDescriptor = group.FirstOrDefault(d => d.MiddlewareType == dependency);

                if (dependencyDescriptor != null)
                {
                    graph[descriptor].Add(dependencyDescriptor);
                }
            }

            // Add preceding middleware
            foreach (var preceding in descriptor.PrecedingMiddleware ?? Enumerable.Empty<Type>())
            {
                var precedingDescriptor = group.FirstOrDefault(d => d.MiddlewareType == preceding);

                if (precedingDescriptor != null)
                {
                    graph[descriptor].Add(precedingDescriptor);
                }
            }

            // Add following middleware
            foreach (var following in descriptor.FollowingMiddleware ?? Enumerable.Empty<Type>())
            {
                var followingDescriptor = group.FirstOrDefault(d => d.MiddlewareType == following);

                if (followingDescriptor != null)
                {
                    if (!graph.ContainsKey(followingDescriptor))
                    {
                        graph[followingDescriptor] = new List<MiddlewareDescriptor>();
                    }

                    graph[followingDescriptor].Add(descriptor);
                }
            }
        }

        return graph;
    }

    private List<string> SortGroups(Dictionary<string, List<MiddlewareDescriptor>> groups)
    {
        // Simple example assumes no dependencies between groups
        // Customize if group dependencies are introduced
        return groups.Keys.OrderBy(g => g).ToList();
    }

    private List<MiddlewareDescriptor> TopologicalSort(Dictionary<MiddlewareDescriptor, List<MiddlewareDescriptor>> graph)
    {
        var sorted = new List<MiddlewareDescriptor>();
        var visited = new HashSet<MiddlewareDescriptor>();
        var visiting = new HashSet<MiddlewareDescriptor>();

        foreach (var node in graph.Keys)
        {
            Visit(node, graph, sorted, visited, visiting);
        }

        return sorted;
    }

    private void Visit(
        MiddlewareDescriptor node,
        Dictionary<MiddlewareDescriptor, List<MiddlewareDescriptor>> graph,
        List<MiddlewareDescriptor> sorted,
        HashSet<MiddlewareDescriptor> visited,
        HashSet<MiddlewareDescriptor> visiting)
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

    internal IEnumerable<MiddlewareDescriptor> GetMiddlewareDescriptors() => _middlewares.AsEnumerable();
    #endregion

    #region Middleware Binding
    internal class MiddlewareBinding : IMiddlewareBinding
    {
        private readonly IServiceCollection _services;
        private readonly object _application;
        internal readonly MiddlewareDescriptor _descriptor;

        public MiddlewareBinding(IServiceCollection services, object application, MiddlewareDescriptor descriptor)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public IMiddlewareBinding WithInstance(object instance)
        {
            ArgumentGuard.NotNull(instance, nameof(instance));

            _descriptor.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            return this;
        }

        public IMiddlewareBinding WithPriority(int priority)
        {
            ArgumentGuard.NotNegativeOrZero(priority, nameof(priority));

            _descriptor.Priority = priority;
            return this;
        }

        public IMiddlewareBinding WithPriority(Func<int> priority)
        {
            ArgumentGuard.NotNull(priority, nameof(priority));

            var value = priority();

            ArgumentGuard.NotNegativeOrZero(value, nameof(priority));

            _descriptor.Priority = value;
            return this;
        }

        public IMiddlewareBinding WithPriority<TContext>(Func<TContext, int> priority)
        {
            ArgumentGuard.NotNull(priority, nameof(priority));

            // TODO: retrieve TContext from DI container
            // Assume some context or service is provided to calculate priority
            var context = Activator.CreateInstance<TContext>();

            if (context is null)
            {
                throw new InvalidOperationException("Could not create instance of context type.");
            }

            var value = priority(context);

            ArgumentGuard.NotNegativeOrZero(value, nameof(priority));

            _descriptor.Priority = value;
            return this;
        }

        public IMiddlewareBinding WithExecutionPolicy<T>(Action<T> configurePolicy) where T : class
        {
            ArgumentGuard.NotNull(configurePolicy, nameof(configurePolicy));

            var policy = Activator.CreateInstance<T>();
            configurePolicy(policy);
            _descriptor.ExecutionPolicy = policy;
            return this;
        }

        public IMiddlewareBinding WithMetadata<TMetadata>(TMetadata metadata)
        {
            _descriptor.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            return this;
        }

        public IMiddlewareBinding WithFallback(Func<object, Task> fallback)
        {
            ArgumentGuard.NotNull(fallback, nameof(fallback));

            _descriptor.Fallback = middleware => fallback(middleware);
            return this;
        }

        public IMiddlewareBinding WithOptions<TOptions>(TOptions options) where TOptions : class
        {
            _descriptor.Options = options ?? throw new ArgumentNullException(nameof(options));
            return this;
        }

        public IMiddlewareBinding WithTag(string tag)
        {
            ArgumentGuard.NotNullOrEmpty(tag, nameof(tag));

            _descriptor.Tag = tag;
            return this;
        }

        public IMiddlewareBinding When(Func<bool> condition)
        {
            ArgumentGuard.NotNull(condition, nameof(condition));

            _descriptor.Condition = condition;
            return this;
        }

        public IMiddlewareBinding When<TContext>(Func<TContext, bool> condition)
        {
            ArgumentGuard.NotNull(condition, nameof(condition));

            // Assume some context is provided to evaluate the condition
            var context = Activator.CreateInstance<TContext>();
            _descriptor.Condition = () => condition(context);
            return this;
        }

        public IMiddlewareBinding InGroup(string group)
        {
            ArgumentGuard.NotNullOrEmpty(group, nameof(group));

            _descriptor.Group = group;
            return this;
        }

        public IMiddlewareBinding DependsOn<TOtherMiddleware>()
        {
            _descriptor.Dependencies ??= new List<Type>();
            _descriptor.Dependencies.Add(typeof(TOtherMiddleware));
            return this;
        }

        public IMiddlewareBinding Precedes<TPrecedingMiddleware>()
        {
            _descriptor.PrecedingMiddleware ??= new List<Type>();
            _descriptor.PrecedingMiddleware.Add(typeof(TPrecedingMiddleware));
            return this;
        }

        public IMiddlewareBinding Follows<TFollowingMiddleware>()
        {
            _descriptor.FollowingMiddleware ??= new List<Type>();
            _descriptor.FollowingMiddleware.Add(typeof(TFollowingMiddleware));
            return this;
        }

        public IMiddlewareBinding Disable()
        {
            _descriptor.IsEnabled = false;
            return this;
        }

        public IMiddlewareBinding Enable()
        {
            _descriptor.IsEnabled = true;
            return this;
        }

        public IMiddlewareBinding RequireEnvironment(string environment)
        {
            ArgumentGuard.NotNullOrEmpty(environment, nameof(environment));

            _descriptor.RequiredEnvironment = environment;
            return this;
        }

        public IMiddlewareBinding WithTimeout(TimeSpan timeout)
        {
            _descriptor.Timeout = timeout;
            return this;
        }

        public IMiddlewareBinding OnError(Func<Exception, Task> errorHandler)
        {
            ArgumentGuard.NotNull(errorHandler, nameof(errorHandler));

            _descriptor.ErrorHandler = errorHandler;
            return this;
        }
    }
    #endregion
}
