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

    public TApplication Application => _application;

    public MiddlewareDescriptor? CurrentDescriptor { get; private set; }

    public MiddlewareConfigurator(IServiceCollection services, TApplication application)
    {
        _services = services;
        _application = application;
    }

    public IMiddlewareBinding<TMiddleware, TApplication> UseMiddleware<TMiddleware>() where TMiddleware : class
    {
        var descriptor = new MiddlewareDescriptor(typeof(TMiddleware));
        CurrentDescriptor = descriptor;

        _middlewares.Add(descriptor);
        return new MiddlewareBinding<TMiddleware, TApplication>(_services, _application, descriptor);
    }

    public IMiddlewareBinding<TMiddleware, TApplication> RemoveMiddleware<TMiddleware>() where TMiddleware : class
    {
        var descriptor = new MiddlewareDescriptor(typeof(TMiddleware));

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Middleware of type {typeof(TMiddleware).Name} is not registered.");
        }

        _middlewares.Remove(descriptor);

        return new MiddlewareBinding<TMiddleware, TApplication>(_services, _application, descriptor);
    }

    public IMiddlewareBinding<TMiddleware, TApplication> GetMiddleware<TMiddleware>() where TMiddleware : class
    {
        var descriptor = _middlewares.FirstOrDefault(m => m.MiddlewareType == typeof(TMiddleware));

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Middleware of type {typeof(TMiddleware).Name} is not registered.");
        }

        return new MiddlewareBinding<TMiddleware, TApplication>(_services, _application, descriptor);
    }

    public void ApplyGroupPolicy<TMiddleware>(string groupName, Action<IMiddlewareBinding<TMiddleware, TApplication>> configure) where TMiddleware : class
    {
        var groupMiddlewares = _middlewares.Where(m => m.Group == groupName);

        foreach (var descriptor in groupMiddlewares)
        {
            configure(new MiddlewareBinding<TMiddleware, TApplication>(_services, _application, descriptor));
        }
    }

    public void ConfigureAll<TMiddleware>(Action<IMiddlewareBinding<TMiddleware, TApplication>> configure) where TMiddleware : class
    {
        var allMiddlewares = _middlewares;

        foreach (var descriptor in allMiddlewares)
        {
            configure(new MiddlewareBinding<TMiddleware, TApplication>(_services, _application, descriptor));
        }
    }

    public void Register()
    {
        Debug.WriteLine("Middlewares in configurator:");
        foreach (var descriptor in _middlewares)
        {
            Debug.WriteLine($"Middleware: {descriptor.MiddlewareType}, " +
                              $"Priority: {descriptor.Priority}, " +
                              $"Group: {descriptor.Group}");
        }

        var application = _application as IApplicationBuilder;
        var sp = application!.ApplicationServices;

        // Order middleware descriptors
        var orderedMiddlewares = OrderMiddlewareDescriptors();

        // Add middleware to the pipeline in the correct order
        foreach (var descriptor in orderedMiddlewares)
        {
            if (descriptor.IsEnabled == false || (descriptor.Condition != null && !descriptor.Condition.Invoke()))
                continue;

            application.Use(async (HttpContext context, Func<Task> next) =>
            {
                var middlewareInstance = application.ApplicationServices.GetRequiredService(descriptor.MiddlewareType!);
                await InvokeMiddleware(middlewareInstance, context, next);
            });

            Debug.WriteLine($"Registered middleware: {descriptor.MiddlewareType.FullName}, Priority: {descriptor.Priority}");
        }
    }

    private static async Task InvokeMiddleware(object middleware, HttpContext context, Func<Task> next)
    {
        // Locate the "Invoke" method dynamically, assuming it's conventionally named
        var method = middleware.GetType().GetMethod("InvokeAsync") ?? middleware.GetType().GetMethod("Invoke");

        if (method == null)
        {
            throw new InvalidOperationException($"Middleware {middleware.GetType().FullName} does not have an Invoke or InvokeAsync method.");
        }

        // Dynamically invoke the middleware's method
        var parameters = method.GetParameters();
        var args = parameters.Length == 2
            ? new object[] { context, next }
            : new object[] { context };

        await (Task)method.Invoke(middleware, args)!;
    }

    #region Middleware Ordering
    private List<MiddlewareDescriptor> OrderMiddlewareDescriptors()
    {
        var graph = new Dictionary<MiddlewareDescriptor, List<MiddlewareDescriptor>>();

        // Initialize graph with all descriptors
        foreach (var descriptor in _middlewares)
        {
            graph[descriptor] = new List<MiddlewareDescriptor>();
        }

        // Build the graph based on dependencies, preceding, and following relationships
        foreach (var descriptor in _middlewares)
        {
            if (descriptor.Dependencies != null)
            {
                foreach (var dependencyType in descriptor.Dependencies)
                {
                    var dependency = FindMiddlewareDescriptor(dependencyType);
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
                    var precedingMiddleware = FindMiddlewareDescriptor(precedingType);
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
                    var followingMiddleware = FindMiddlewareDescriptor(followingType);
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

    private MiddlewareDescriptor? FindMiddlewareDescriptor(Type middlewareType)
    {
        return _middlewares.FirstOrDefault(d => d.MiddlewareType == middlewareType);
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
    private class MiddlewareBinding<TMiddleware, TOuterApplication> : IMiddlewareBinding<TMiddleware, TOuterApplication>
        where TMiddleware : class
        where TOuterApplication : class
    {
        private readonly IServiceCollection _services;
        private readonly TOuterApplication _application;
        private readonly MiddlewareDescriptor _descriptor;

        public MiddlewareBinding(IServiceCollection services, TOuterApplication application, MiddlewareDescriptor descriptor)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithPriority(int priority)
        {
            _descriptor.Priority = priority;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithPriority(Func<int> priority)
        {
            ArgumentGuard.NotNull(priority, nameof(priority));

            _descriptor.Priority = priority();
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithPriority<TContext>(Func<TContext, int> priority)
        {
            ArgumentGuard.NotNull(priority, nameof(priority));

            // TODO: retrieve TContext from DI container
            // Assume some context or service is provided to calculate priority
            var context = Activator.CreateInstance<TContext>();
            _descriptor.Priority = priority(context);
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithExecutionPolicy<T>(Action<T> configurePolicy) where T : class
        {
            var policy = Activator.CreateInstance<T>();
            configurePolicy(policy);
            _descriptor.ExecutionPolicy = policy;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithMetadata<TMetadata>(TMetadata metadata)
        {
            _descriptor.Metadata = metadata;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithFallback(Func<TMiddleware, Task> fallback)
        {
            _descriptor.Fallback = middleware => fallback((TMiddleware)middleware);
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithOptions<TOptions>(TOptions options) where TOptions : class
        {
            _descriptor.Options = options;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithTag(string tag)
        {
            _descriptor.Tag = tag;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> When(Func<bool> condition)
        {
            _descriptor.Condition = condition;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> When<TContext>(Func<TContext, bool> condition)
        {
            // Assume some context is provided to evaluate the condition
            var context = Activator.CreateInstance<TContext>();
            _descriptor.Condition = () => condition(context);
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> InGroup(string group)
        {
            _descriptor.Group = group;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> DependsOn<TOtherMiddleware>()
        {
            _descriptor.Dependencies ??= new List<Type>();
            _descriptor.Dependencies.Add(typeof(TOtherMiddleware));
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> Precedes<TPrecedingMiddleware>()
        {
            _descriptor.PrecedingMiddleware ??= new List<Type>();
            _descriptor.PrecedingMiddleware.Add(typeof(TPrecedingMiddleware));
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> Follows<TFollowingMiddleware>()
        {
            _descriptor.FollowingMiddleware ??= new List<Type>();
            _descriptor.FollowingMiddleware.Add(typeof(TFollowingMiddleware));
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> Disable()
        {
            _descriptor.IsEnabled = false;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> Enable()
        {
            _descriptor.IsEnabled = true;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> RequireEnvironment(string environment)
        {
            _descriptor.RequiredEnvironment = environment;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> WithTimeout(TimeSpan timeout)
        {
            _descriptor.Timeout = timeout;
            return this;
        }

        public IMiddlewareBinding<TMiddleware, TOuterApplication> OnError(Func<Exception, Task> errorHandler)
        {
            _descriptor.ErrorHandler = errorHandler;
            return this;
        }
    }
    #endregion
}
