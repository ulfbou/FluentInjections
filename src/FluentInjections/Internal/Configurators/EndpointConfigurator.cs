// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

internal class EndpointConfigurator : Configurator<IEndpointBinding, EndpointDescriptor>, IEndpointConfigurator
{
    public EndpointConfigurator(ILogger logger) : base(logger) { }

    public IEndpointsBuilder Map<TService>(string pattern, EndpointMethod method) where TService : class
    {
        return new EndpointsBuilder<TService>(pattern, method, AddBinding);
    }

    private void AddBinding(EndpointDescriptor binding)
    {
        _descriptors.Add(binding);
    }

    public IReadOnlyList<IEndpointDescriptor> GetBindings() => _descriptors.AsReadOnly();
    protected override void Register(EndpointDescriptor descriptor) => throw new NotImplementedException();
    protected internal override void ValidateBindings() => throw new NotImplementedException();

    internal class EndpointsBuilder<TService> : IEndpointsBuilder where TService : class
    {
        private readonly string _pattern;
        private readonly EndpointMethod _method;
        private readonly Action<EndpointDescriptor> _addBinding;
        private readonly EndpointDescriptor _descriptor;

        public EndpointsBuilder(string pattern, EndpointMethod method, Action<EndpointDescriptor> addBinding)
        {
            _pattern = pattern;
            _method = method;
            _addBinding = addBinding;
            _descriptor = new EndpointDescriptor
            {
                Pattern = pattern,
                Method = method,
                ServiceType = typeof(TService),
                Metadata = new Dictionary<string, object>()
            };
        }

        public IEndpointsBuilder RequireAuthorization()
        {
            _descriptor.RequireAuthorization = true;
            return this;
        }

        public IEndpointsBuilder UseValidation<TValidationFilter>(Action<TValidationFilter>? configure = null)
            where TValidationFilter : class
        {
            _descriptor.ValidationFilterType = typeof(TValidationFilter);
            _descriptor.ValidationFilterInstance = configure != null ? Activator.CreateInstance<TValidationFilter>() : null;
            configure?.Invoke((TValidationFilter)_descriptor.ValidationFilterInstance!);
            return this;
        }

        public IEndpointsBuilder WithName(string endpointName)
        {
            _descriptor.Name = endpointName;
            return this;
        }

        public IEndpointsBuilder InGroup(string groupName)
        {
            _descriptor.Group = groupName;
            return this;
        }

        public IEndpointsBuilder OnError(Func<Exception, Task> errorHandler)
        {
            _descriptor.ErrorHandler = errorHandler;
            return this;
        }

        public IEndpointsBuilder WithPriority(int priority)
        {
            _descriptor.Priority = priority;
            return this;
        }

        public IEndpointsBuilder WithTag(string tag)
        {
            _descriptor.Tag = tag;
            return this;
        }

        public IEndpointsBuilder WithTimeout(TimeSpan timeout)
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
