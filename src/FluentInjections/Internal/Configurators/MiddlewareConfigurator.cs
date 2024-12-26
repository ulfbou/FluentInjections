using Autofac;
using Autofac.Core.Registration;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Internal.Configurators;

internal class MiddlewareConfigurator : IMiddlewareConfigurator
{
    private readonly ContainerBuilder _builder;
    private object? _middleware;
    private Type? _middlewareType;
    private List<MiddlewareBindingDescriptor> _descriptors = new();
    private List<IMiddlewareBinding> _bindings = new();

    public object Middleware => _middleware!;
    public Type MiddlewareType => _middlewareType!;
    internal IReadOnlyList<MiddlewareBindingDescriptor> MiddlewareBindings => _descriptors.AsReadOnly();

    public MiddlewareConfigurator(ContainerBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public IMiddlewareBinding GetMiddleware<TMiddleware>() where TMiddleware : class => GetMiddleware(typeof(TMiddleware));
    public IMiddlewareBinding GetMiddleware(Type middleware) => (_bindings.FirstOrDefault(b => b.Equals(middleware)) as IMiddlewareBinding)!;
    public IMiddlewareBinding RemoveMiddleware<TMiddleware>() where TMiddleware : class
        => RemoveMiddleware(typeof(TMiddleware));

    public IMiddlewareBinding RemoveMiddleware(Type middleware)
    {
        ArgumentGuard.NotNull(middleware, nameof(middleware));

        var binding = GetMiddleware(middleware);
        _descriptors.Remove(binding.Descriptor);
        _bindings.Remove(binding);
        return binding;
    }

    public IMiddlewareBinding UseMiddleware<TMiddleware>() where TMiddleware : class => UseMiddleware(typeof(TMiddleware));
    public IMiddlewareBinding UseMiddleware(Type middleware)
    {
        ArgumentGuard.NotNull(middleware, nameof(middleware));

        _middleware = null;
        _middlewareType = middleware;
        var descriptor = new MiddlewareBindingDescriptor(_middlewareType);
        var binding = new MiddlewareBinding(descriptor);
        _descriptors.Add(descriptor);
        _bindings.Add(binding);
        return binding;
    }

    public void ApplyGroupPolicy(string groupName, Action<IMiddlewareBinding> configure)
    {
        ArgumentGuard.NotNullOrWhiteSpace(groupName, nameof(groupName));
        ArgumentGuard.NotNull(configure, nameof(configure));

        var bindings = _bindings.Where(b => b.Descriptor.Group == groupName);

        foreach (var binding in bindings)
        {
            configure(binding);
        }
    }

    public void ConfigureAll(Action<IMiddlewareBinding> configure)
    {
        ArgumentGuard.NotNull(configure, nameof(configure));

        foreach (var binding in _bindings)
        {
            configure(binding);
        }
    }

    public void Register() => _descriptors.ForEach(d => Register(d));

    internal void Register(Action<MiddlewareBindingDescriptor, HttpContext, IApplicationBuilder> register) => _descriptors.ForEach(d => Register(d, register));

    // TODO: Implement the registration of middleware components correctly.
    private void Register(MiddlewareBindingDescriptor descriptor, Action<MiddlewareBindingDescriptor, HttpContext, IApplicationBuilder>? register = null)
    {
        if (descriptor.Instance is not null)
        {
            _builder.RegisterInstance(descriptor.Instance).As(descriptor.MiddlewareType).SingleInstance();
        }
        else
        {
            _builder.Register(ctx =>
            {
                var middleware = ctx.Resolve(descriptor.MiddlewareType);
                return middleware;
            }).As(descriptor.MiddlewareType).InstancePerLifetimeScope();
        }
    }

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
        public TMiddleware Middleware { get; private set; }

        public MiddlewareBinding(MiddlewareBindingDescriptor descriptor, TMiddleware middleware)
            : base(descriptor)
        {
            Middleware = middleware;
        }

        public IMiddlewareBinding<TMiddleware> DependsOn<TOtherMiddleware>()
        {
            Descriptor.Dependencies.Add(typeof(TOtherMiddleware));
            return this;
        }

        public IMiddlewareBinding<TMiddleware> Disable()
        {
            Descriptor.IsEnabled = false;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> Enable()
        {
            Descriptor.IsEnabled = true;
            return this;
        }
        public IMiddlewareBinding<TMiddleware> Follows<TFollowingMiddleware>()
        {
            Descriptor.FollowingMiddleware.Add(typeof(TFollowingMiddleware));
            return this;
        }

        public IMiddlewareBinding<TMiddleware> InGroup(string group)
        {
            ArgumentGuard.NotNullOrWhiteSpace(group, nameof(group));

            Descriptor.Group = group;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> OnError(Func<Exception, Task> errorHandler)
        {
            ArgumentGuard.NotNull(errorHandler, nameof(errorHandler));

            Descriptor.ErrorHandler = errorHandler;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> Precedes<TPrecedingMiddleware>()
        {
            Descriptor.PrecedingMiddleware.Add(typeof(TPrecedingMiddleware));
            return this;
        }

        public IMiddlewareBinding<TMiddleware> RequireEnvironment(string environment)
        {
            ArgumentGuard.NotNullOrWhiteSpace(environment, nameof(environment));

            Descriptor.Environment = environment;
            return this;
        }
        public IMiddlewareBinding<TMiddleware> When(Func<bool> func)
        {
            ArgumentGuard.NotNull(func, nameof(func));

            Descriptor.Condition = func;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> When<TContext>(Func<TContext, bool> func)
        {
            ArgumentGuard.NotNull(func, nameof(func));

            // TODO: Handle context-based conditions correctly. 
            Descriptor.Condition = () => func(default!);
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithExecutionPolicy<TPolicy>(Action<TPolicy> value) where TPolicy : class
        {
            ArgumentGuard.NotNull(value, nameof(value));

            Descriptor.ExecutionPolicy = value;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithFallback(Func<object, Task> fallback)
        {
            ArgumentGuard.NotNull(fallback, nameof(fallback));

            Descriptor.Fallback = fallback;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithInstance(object instance)
        {
            var middleware = instance as TMiddleware;

            if (middleware == null)
            {
                throw new ArgumentException($"The instance must be of type {typeof(TMiddleware).Name}.", nameof(instance));
            }

            Middleware = middleware;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithMetadata<TMetadata>(TMetadata metadata)
        {
            ArgumentGuard.NotNull(metadata, nameof(metadata));

            Descriptor.Metadata = metadata;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithOptions<TOptions>(TOptions options) where TOptions : class
        {
            ArgumentGuard.NotNull(options, nameof(options));

            Descriptor.Options = options;
            Descriptor.OptionsType = typeof(TOptions);
            return this;
        }
        public IMiddlewareBinding<TMiddleware> WithPriority(int priority)
        {
            Descriptor.Priority = priority;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithPriority(Func<int> priority)
        {
            ArgumentGuard.NotNull(priority, nameof(priority));

            Descriptor.Priority = priority();
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithPriority<TContext>(Func<TContext, int> priority)
        {
            ArgumentGuard.NotNull(priority, nameof(priority));

            // TODO: Handle context-based priorities correctly.
            var context = default(TContext);
            Descriptor.Priority = priority(context!);
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithTag(string tag)
        {
            ArgumentGuard.NotNullOrWhiteSpace(tag, nameof(tag));

            Descriptor.Tag = tag;
            return this;
        }

        public IMiddlewareBinding<TMiddleware> WithTimeout(TimeSpan timeout)
        {
            Descriptor.Timeout = timeout;
            return this;
        }
    }
}
