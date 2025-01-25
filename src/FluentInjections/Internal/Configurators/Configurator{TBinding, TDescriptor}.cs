// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

/// <summary>
/// Represents a configurator that provides methods to bind and configure services within the application.
/// </summary>
internal abstract class BaseConfigurator<TBinding, TDescriptor> : IConfigurator<TBinding> where TBinding : IBinding
{
    protected readonly List<TDescriptor> _descriptors = new();
    protected readonly ILogger _logger;

    protected internal IReadOnlyList<TDescriptor> Descriptors => _descriptors.AsReadOnly();
    protected internal ILogger Logger => _logger;

    protected BaseConfigurator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets or sets the conflict resolution mode for the configurator.
    /// </summary>
    public ConflictResolutionMode ConflictResolution
    {
        get => _conflictResolution;
        set
        {
            if (!Enum.IsDefined(typeof(ConflictResolutionMode), value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The value must be a valid ConflictResolutionMode.");
            }

            _conflictResolution = value;
        }
    }
    private ConflictResolutionMode _conflictResolution = ConflictResolutionMode.WarnAndReplace;

    /// <inheritdoc/>
    public void Register()
    {
        ValidateBindings();

        List<TDescriptor> orderedDescriptors = OrderBindingDescriptors();

        orderedDescriptors.ForEach(d => Register(d));
    }

    /// <summary>
    /// Registers a binding with the service collection.
    /// </summary>
    /// <param name="descriptor">The binding descriptor to register.</param>
    protected abstract void Register(TDescriptor descriptor, int? priority = int.MaxValue);

    /// <summary>
    /// Validates the bindings to ensure they are configured correctly.
    /// </summary>
    protected internal abstract void ValidateBindings();

    /// <summary>
    /// Orders the binding descriptors.
    /// </summary>
    /// <returns>The ordered list of binding descriptors.</returns>
    protected virtual List<TDescriptor> OrderBindingDescriptors() => _descriptors;

    /// <inheritdoc/>
    public void Dispose()
    {
        _descriptors.Clear();
    }
}
