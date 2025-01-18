// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Extensions;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

internal class EndpointConfigurator : Configurator<IEndpointBinding, EndpointDescriptor>, IEndpointConfigurator
{
    public WebApplication App { get; }
    public IRouteBuilder Builder { get; }

    public EndpointConfigurator(WebApplication application, ILogger logger) : base(logger)
    {
        App = application ?? throw new ArgumentNullException(nameof(application));
        Builder = new RouteBuilder(App);
    }

    public IEndpointsBuilder<TRequest> Map<TService, TRequest>(
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

    public IReadOnlyList<IEndpointDescriptor> GetBindings() => _descriptors.AsReadOnly();

    // Register the endpoint with the application
    protected override void Register(EndpointDescriptor descriptor)
    {
        var service = App.Services.GetRequiredService(descriptor.ServiceType);

        var config = Builder.MapVerb(descriptor.Pattern, descriptor.Method.ToString(), async (HttpContext context) =>
        {
            object? requestBody = null;
            try
            {
                requestBody = await context.Request.ReadFromJsonAsync(descriptor.Handler!.Method.GetParameters()[1].ParameterType);
            }
            catch (Exception ex)
            {
                if (descriptor.ErrorHandler != null)
                {
                    await descriptor.ErrorHandler(ex);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("An error occurred while processing the request.");
                }
                return;
            }

            if (requestBody == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid request body.");
                return;
            }

            try
            {
                var result = await (Task<IResult>)descriptor.Handler!.DynamicInvoke(service, requestBody, context)!;
                await result.ExecuteAsync(context);
            }
            catch (Exception ex)
            {
                if (descriptor.ErrorHandler != null)
                {
                    await descriptor.ErrorHandler(ex);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("An error occurred while processing the request.");
                }
            }
        });

        if (descriptor.RequireAuthorization)
        {
            config.RequireAuthorization();
        }

        ApplyDescriptorPolicies(descriptor);
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

    internal class EndpointsBuilder<TService, TRequest> : IEndpointsBuilder<TRequest> where TService : class
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

        public IEndpointsBuilder<TRequest> RequireAuthorization()
        {
            _descriptor.RequireAuthorization = true;
            return this;
        }

        public IEndpointsBuilder<TRequest> ConfigureValidation<TValidationFilter>(Action<TValidationFilter>? configure = null)
            where TValidationFilter : class
        {
            _descriptor.ValidationFilterType = typeof(TValidationFilter);
            _descriptor.ValidationFilterInstance = configure != null ? Activator.CreateInstance<TValidationFilter>() : null;
            configure?.Invoke((TValidationFilter)_descriptor.ValidationFilterInstance!);
            return this;
        }

        public IEndpointsBuilder<TRequest> WithName(string endpointName)
        {
            _descriptor.Name = endpointName;
            return this;
        }

        public IEndpointsBuilder<TRequest> WithGroup(string groupName)
        {
            _descriptor.Group = groupName;
            return this;
        }

        public IEndpointsBuilder<TRequest> WithErrorHandler(Func<Exception, Task> errorHandler)
        {
            _descriptor.ErrorHandler = errorHandler;
            return this;
        }

        public IEndpointsBuilder<TRequest> WithPriority(int priority)
        {
            _descriptor.Priority = priority;
            return this;
        }

        public IEndpointsBuilder<TRequest> WithTag(string tag)
        {
            _descriptor.Tag = tag;
            return this;
        }

        public IEndpointsBuilder<TRequest> WithTimeout(TimeSpan timeout)
        {
            _descriptor.Timeout = timeout;
            return this;
        }

        public void ApplyGroupPolicy(string groupName, Action<IEndpointDescriptor> configure)
        {
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

        public void Build()
        {
            _addBinding(_descriptor);
        }
    }
}
