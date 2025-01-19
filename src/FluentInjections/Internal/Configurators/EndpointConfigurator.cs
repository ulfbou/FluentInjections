// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Constants;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Extensions;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace FluentInjections.Internal.Configurators;

internal class EndpointConfigurator : Configurator<IEndpointBinding, EndpointDescriptor>, IEndpointConfigurator
{
    protected readonly Dictionary<string, RouteGroupBuilder> _routeGroupBuilders = new();
    public WebApplication App { get; }

    public EndpointConfigurator(WebApplication application, ILogger logger) : base(logger)
    {
        App = application ?? throw new ArgumentNullException(nameof(application));
        _routeGroupBuilders.Add(Endpoints.Routes.DefaultGroup, App.MapGroup(Endpoints.Routes.DefaultGroup));
    }

    public IEndpointsBuilder<TService, TRequest> Map<TService, TRequest>(
        string pattern,
        EndpointMethod method,
        Func<TService, TRequest, HttpContext, Task<IResult>> handler)
        where TService : class
    {
        return new EndpointsBuilder<TService, TRequest>(pattern, method, handler, AddBinding);
    }

    private void AddBinding(EndpointDescriptor binding)
    {
        _descriptors.Add(binding);
    }

    // Register the endpoint with the application
    protected override void Register(EndpointDescriptor descriptor)
    {
        var service = App.Services.GetRequiredService(descriptor.ServiceType);
        var group = GetOrCreateGroup(descriptor.Group);

        if (descriptor.Priority > 0)
        {
            descriptor.TryAddMetadata(Endpoints.Descriptor.PriorityKey, descriptor.Priority);
        }


        if (descriptor.Timeout.HasValue)
        {
            descriptor.TryAddMetadata(Endpoints.Descriptor.TimeoutKey, descriptor.Timeout.Value);
        }

        var config = group.MapVerb(descriptor.Pattern, descriptor.Method.ToString(), builder =>
        {
            if (descriptor.Metadata.Count > 0)
            {
                foreach (var metadata in descriptor.Metadata)
                {
                    builder.WithMetadata(metadata.Value);
                }
            }

            if (!string.IsNullOrEmpty(descriptor.Tag))
            {
                builder.WithTags(descriptor.Tag);
            }

            if (!string.IsNullOrEmpty(descriptor.Name))
            {
                builder.WithName(descriptor.Name);
            }

            if (!string.IsNullOrEmpty(descriptor.Group))
            {
                builder.WithGroupName(descriptor.Group);
            }

        });



        if (descriptor.RequireAuthorization)
        {
            config.RequireAuthorization();
        }

        ApplyDescriptorPolicies(descriptor);
    }

    private RouteGroupBuilder GetOrCreateGroup(string? groupName)
    {
        RouteGroupBuilder groupBuilder = default!;

        if (string.IsNullOrEmpty(groupName))
        {
            if (_routeGroupBuilders.Count == 0)
            {
                Debug.WriteLine("No group specified and no group defined. Creating a group for ");
                groupBuilder = App.MapGroup(Endpoints.Routes.DefaultGroup);
                _routeGroupBuilders.Add(Endpoints.Routes.DefaultGroup, groupBuilder);
                return groupBuilder;
            }

            Debug.WriteLine("No group specified. Using the default group.");
            return _routeGroupBuilders.TryGetValue(Endpoints.Routes.DefaultGroup, out groupBuilder!) ? groupBuilder : default!;
        }

        var existingGroup = _routeGroupBuilders.GetValueOrDefault(groupName);

        if (existingGroup is not null)
        {
            return groupBuilder;
        }

        groupBuilder = App.MapGroup(groupName);
        _routeGroupBuilders.Add(groupName, groupBuilder);
        return groupBuilder;
    }

    private void ApplyDescriptorPolicies(EndpointDescriptor descriptor)
    {
        // Apply additional policies or configurations based on the descriptor properties if needed
        // For example, handling metadata, validation filters, etc.
        if (descriptor.ValidationFilterInstance != null)
        {
            // Apply the validation filter instance
            // You can implement your own logic to handle the validation
        }
    }


    protected internal override void ValidateBindings()
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj) => obj is EndpointConfigurator configurator && ReferenceEquals(this, configurator);

    internal class EndpointsBuilder<TService, TRequest> : IEndpointsBuilder<TService, TRequest> where TService : class
    {
        private readonly string _pattern;
        private readonly EndpointMethod _method;
        private readonly Func<TService, TRequest, HttpContext, Task<IResult>> _handler;
        private readonly Action<EndpointDescriptor> _addBinding;
        private readonly EndpointDescriptor _descriptor;

        public EndpointsBuilder(
            string pattern,
            EndpointMethod method,
            Func<TService, TRequest, HttpContext, Task<IResult>> handler,
            Action<EndpointDescriptor> addBinding)
        {
            _pattern = pattern;
            _method = method;
            _handler = handler;
            _addBinding = addBinding;
            _descriptor = new EndpointDescriptor
            {
                Pattern = pattern,
                Method = method,
                ServiceType = typeof(TService),
                Handler = (service, request, context) => handler((TService)service, (TRequest)request, (HttpContext)context),
                Metadata = new Dictionary<string, object>()
            };
        }

        public IEndpointsBuilder<TService, TRequest> RequireAuthorization()
        {
            _descriptor.RequireAuthorization = true;
            return this;
        }

        public IEndpointsBuilder<TService, TRequest> ConfigureValidation<TValidationFilter>(Action<TValidationFilter>? configure = null)
            where TValidationFilter : class
        {
            _descriptor.ValidationFilterType = typeof(TValidationFilter);
            _descriptor.ValidationFilterInstance = configure != null ? Activator.CreateInstance<TValidationFilter>() : null;
            configure?.Invoke((TValidationFilter)_descriptor.ValidationFilterInstance!);
            return this;
        }

        public IEndpointsBuilder<TService, TRequest> WithName(string endpointName)
        {
            _descriptor.Name = endpointName;
            return this;
        }

        public IEndpointsBuilder<TService, TRequest> WithGroupName(string groupName)
        {
            _descriptor.Group = groupName;
            return this;
        }

        public IEndpointsBuilder<TService, TRequest> WithErrorHandler(Func<Exception, Task> errorHandler)
        {
            _descriptor.ErrorHandler = errorHandler;
            return this;
        }

        public IEndpointsBuilder<TService, TRequest> WithPriority(int priority)
        {
            _descriptor.Priority = priority;
            return this;
        }

        public IEndpointsBuilder<TService, TRequest> WithTag(string tag)
        {
            _descriptor.Tag = tag;
            return this;
        }

        /// <inheritdoc/>
        public IEndpointsBuilder<TService, TRequest> WithTimeout(TimeSpan timeout)
        {
            _descriptor.Timeout = timeout;
            return this;
        }

        /// <inheritdoc/>
        public IEndpointsBuilder<TService, TRequest> WithMetadata(string key, object value)
        {
            Guard.NotNullOrWhiteSpace(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            _descriptor.TryAddMetadata(key, value);
            return this;
        }

        /// <inheritdoc/>
        public void ApplyGroupPolicy(string groupName, Action<IEndpointDescriptor> configure)
        {
            Guard.NotNullOrWhiteSpace(groupName, nameof(groupName));
            Guard.NotNull(configure, nameof(configure));

            if (_descriptor.Group == groupName)
            {
                configure(_descriptor);
            }
        }

        public void ConfigureAll(Action<IEndpointDescriptor> configure)
        {
            Guard.NotNull(configure, nameof(configure));
            configure(_descriptor);
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_routeGroupBuilders, base.GetHashCode());
}
