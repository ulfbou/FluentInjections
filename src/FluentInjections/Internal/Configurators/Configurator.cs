// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

/// <summary>
/// Represents a configurator that provides methods to bind and configure services within the application.
/// </summary>
internal abstract class Configurator<TBinding, TDescriptor> : IConfigurator<TBinding>, IDisposable where TBinding : IBinding
{
    protected readonly List<TDescriptor> _descriptors = new();
    protected readonly ILogger _logger;

    internal IReadOnlyList<TDescriptor> Descriptors => _descriptors.AsReadOnly();
    internal ILogger Logger => _logger;

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

    protected Configurator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void Register()
    {
        ValidateBindings();
        _descriptors.ForEach(d => Register(d));
    }

    /// <summary>
    /// Registers a binding with the service collection.
    /// </summary>
    /// <param name="descriptor">The binding descriptor to register.</param>
    protected abstract void Register(TDescriptor descriptor);

    /// <summary>
    /// Validates the bindings to ensure they are configured correctly.
    /// </summary>
    internal abstract void ValidateBindings();

    /// <inheritdoc/>
    public void Dispose()
    {
        _descriptors.Clear();
    }
}
